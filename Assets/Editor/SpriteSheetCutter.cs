using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Chillquarium.EditorTools
{
    /// <summary>
    /// Turns generated art into sprites. <see cref="Cut"/> splits a sheet with a transparent background into one PNG
    /// per item, and <see cref="ImportAsSprite"/> sets a PNG's import settings so the sprite is a given width in world units.
    /// Run them from the CLI, e.g.
    /// <c>unity command script eval "return Chillquarium.EditorTools.SpriteSheetCutter.Cut(\"Art/fish-sheet.png\", \"Assets/Sprites\", new[] { \"Fish/GuppyBaby\", \"Fish/GuppyAdult\" }, 2);"</c>.
    /// </summary>
    public static class SpriteSheetCutter
    {
        // Pixels below this alpha count as background, so a faint leftover glow does not join two items.
        private const byte SolidAlpha = 25;
        // Pieces smaller than this (in pixels) are specks or detached bits; they join the nearest item instead.
        private const int MinimumItemPixels = 400;
        // Transparent margin kept around each cut, so filtering does not clip the outline.
        private const int Padding = 2;

        /// <summary>
        /// Finds each item as a group of touching solid pixels and writes them to <c>outputFolder/name.png</c>.
        /// The sheet is split into <paramref name="columns"/> equal columns; names go row by row, left to right,
        /// with items in each column ordered from top to bottom. Writes nothing when the item count does not fit the names.
        /// </summary>
        public static string Cut(string sheetPath, string outputFolder, string[] names, int columns)
        {
            var sheet = new Texture2D(2, 2);
            sheet.LoadImage(File.ReadAllBytes(sheetPath));
            var items = FindItems(sheet, out var pieceOfPixel);
            var columnWidth = sheet.width / (float)columns;

            // Texture rows start at the bottom, so a higher yMax means higher up in the sheet.
            var itemsByColumn = Enumerable.Range(0, columns)
                .Select(column => items.Where(item => (int)(item.bounds.center.x / columnWidth) == column).OrderByDescending(item => item.bounds.yMax).ToList())
                .ToList();
            var rows = names.Length / columns;

            if (names.Length % columns != 0 || itemsByColumn.Any(column => column.Count != rows))
            {
                Object.DestroyImmediate(sheet);
                var counts = string.Join(", ", itemsByColumn.Select(column => column.Count));
                return $"Found items per column: {counts}, expected {rows} each. Nothing was written.";
            }

            var report = new StringBuilder();

            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var item = itemsByColumn[column][row];
                    var path = $"{outputFolder}/{names[(row * columns) + column]}.png";
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    var cropped = Crop(sheet, item, pieceOfPixel);
                    File.WriteAllBytes(path, cropped.EncodeToPNG());
                    report.AppendLine($"{path}: {cropped.width}x{cropped.height}");
                    Object.DestroyImmediate(cropped);
                }
            }

            Object.DestroyImmediate(sheet);
            AssetDatabase.Refresh();
            return report.ToString();
        }

        public static string ImportAsSprite(string assetPath, float widthInUnits)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.GetSourceTextureWidthAndHeight(out var pixelWidth, out var pixelHeight);
            importer.spritePixelsPerUnit = pixelWidth / widthInUnits;
            importer.SaveAndReimport();
            return $"{assetPath}: {widthInUnits:0.##} x {pixelHeight / importer.spritePixelsPerUnit:0.##} units";
        }

        // Groups of touching solid pixels ("pieces"), with small pieces merged into the nearest big one.
        // pieceOfPixel tells which piece each pixel belongs to (-1 for background), so a cut copies only its own pixels
        // even when a neighbor's long fin reaches into its bounding box.
        private static List<(RectInt bounds, HashSet<int> pieceIds)> FindItems(Texture2D sheet, out int[] pieceOfPixel)
        {
            var pixels = sheet.GetPixels32();
            var width = sheet.width;
            var height = sheet.height;
            pieceOfPixel = Enumerable.Repeat(-1, pixels.Length).ToArray();
            var pieces = new List<(RectInt bounds, int pixelCount)>();
            var queue = new Queue<int>();

            for (var start = 0; start < pixels.Length; start++)
            {
                if (pieceOfPixel[start] >= 0 || pixels[start].a < SolidAlpha)
                {
                    continue;
                }

                var pieceId = pieces.Count;

                var minColumn = width;
                var minRow = height;
                var maxColumn = 0;
                var maxRow = 0;
                var pixelCount = 0;
                pieceOfPixel[start] = pieceId;
                queue.Enqueue(start);

                while (queue.Count > 0)
                {
                    var index = queue.Dequeue();
                    var column = index % width;
                    var row = index / width;
                    minColumn = Mathf.Min(minColumn, column);
                    maxColumn = Mathf.Max(maxColumn, column);
                    minRow = Mathf.Min(minRow, row);
                    maxRow = Mathf.Max(maxRow, row);
                    pixelCount++;

                    for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
                    {
                        for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
                        {
                            var neighborColumn = column + columnOffset;
                            var neighborRow = row + rowOffset;

                            if (neighborColumn < 0 || neighborColumn >= width || neighborRow < 0 || neighborRow >= height)
                            {
                                continue;
                            }

                            var neighbor = (neighborRow * width) + neighborColumn;

                            if (pieceOfPixel[neighbor] < 0 && pixels[neighbor].a >= SolidAlpha)
                            {
                                pieceOfPixel[neighbor] = pieceId;
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }

                pieces.Add((new RectInt(minColumn, minRow, maxColumn - minColumn + 1, maxRow - minRow + 1), pixelCount));
            }

            var items = Enumerable.Range(0, pieces.Count)
                .Where(pieceId => pieces[pieceId].pixelCount >= MinimumItemPixels)
                .Select(pieceId => (bounds: pieces[pieceId].bounds, pieceIds: new HashSet<int> { pieceId }))
                .ToList();

            for (var pieceId = 0; pieceId < pieces.Count; pieceId++)
            {
                var piece = pieces[pieceId];

                if (piece.pixelCount >= MinimumItemPixels)
                {
                    continue;
                }

                var nearestIndex = Enumerable.Range(0, items.Count).OrderBy(index => Vector2.Distance(items[index].bounds.center, piece.bounds.center)).First();
                var nearest = items[nearestIndex];
                var merged = new RectInt();
                merged.SetMinMax(Vector2Int.Min(nearest.bounds.min, piece.bounds.min), Vector2Int.Max(nearest.bounds.max, piece.bounds.max));
                nearest.pieceIds.Add(pieceId);
                items[nearestIndex] = (merged, nearest.pieceIds);
            }

            return items;
        }

        private static Texture2D Crop(Texture2D sheet, (RectInt bounds, HashSet<int> pieceIds) item, int[] pieceOfPixel)
        {
            var bounds = item.bounds;
            var source = sheet.GetPixels32();
            var croppedPixels = new Color32[(bounds.width + (Padding * 2)) * (bounds.height + (Padding * 2))];
            var croppedWidth = bounds.width + (Padding * 2);

            for (var row = 0; row < bounds.height; row++)
            {
                for (var column = 0; column < bounds.width; column++)
                {
                    var sourceIndex = ((bounds.y + row) * sheet.width) + bounds.x + column;

                    // Faint edge pixels have no piece; keep them only when they touch this item's area, not a neighbor's.
                    var owner = pieceOfPixel[sourceIndex];

                    if (owner < 0 ? IsNextToItem(sourceIndex, sheet.width, sheet.height, pieceOfPixel, item.pieceIds) : item.pieceIds.Contains(owner))
                    {
                        croppedPixels[((row + Padding) * croppedWidth) + column + Padding] = source[sourceIndex];
                    }
                }
            }

            var cropped = new Texture2D(croppedWidth, bounds.height + (Padding * 2), TextureFormat.RGBA32, false);
            cropped.SetPixels32(croppedPixels);
            cropped.Apply();
            return cropped;
        }

        private static bool IsNextToItem(int index, int width, int height, int[] pieceOfPixel, HashSet<int> pieceIds)
        {
            var column = index % width;
            var row = index / width;

            for (var rowOffset = -2; rowOffset <= 2; rowOffset++)
            {
                for (var columnOffset = -2; columnOffset <= 2; columnOffset++)
                {
                    var neighborColumn = column + columnOffset;
                    var neighborRow = row + rowOffset;

                    if (neighborColumn >= 0 && neighborColumn < width && neighborRow >= 0 && neighborRow < height
                        && pieceIds.Contains(pieceOfPixel[(neighborRow * width) + neighborColumn]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
