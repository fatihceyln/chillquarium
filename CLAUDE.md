# Project Rules

## Project overview

- Chillquarium: a 2D game built with Unity **6000.6.0f1**. Render pipeline: URP (2D Renderer, `UniversalRP` + `Renderer2D` in `Assets/Settings/`).
- Target platform: macOS only (Standalone). Do not add mobile, console or other platform settings or assets.
- The game is a learning demo of [Chillquarium](https://play.google.com/store/apps/details?id=com.snapbreak.chillquarium): fish from a market card earn money idly, grow from baby to adult, have rarities and can be fed. It uses the reference game's visual style. The step-by-step plan is in `docs/roadmap.md`. Describe the core loop, main scene and system wiring here as they are built.
- Main scene: `Assets/Scenes/MainScene.unity` (the only scene in the Build Profiles scene list).
- `Main Camera` is orthographic with size 5, so the view is always 10 units tall and its width follows the screen (16 units at 16:10, about 17.8 at 16:9). Keep gameplay content within x ±7.5 so it stays visible on 16:10 Mac screens.
- `Aquarium` holds the tank: `Background` (`Image`, the generated aquarium picture at 17.78 × 10 units, sorting order −7, over flat color bands 36 units wide that only show on screens wider than 16:9; all have negative sorting orders so everything else draws on top) and `SwimArea` (the rectangle fish swim in; it bounds the fish center, so fish longer than about 1 unit need a narrower area).
- Fish exist only at runtime. `Assets/Prefabs/Fish.prefab` shows its species' sprite on `Visual/Sprite`, with a `SortingGroup` so each fish draws as one piece. `Visual` holds the growth size and the facing (sign of x scale); `Sprite` only squeezes during a stroke, so the two never overwrite each other. `Aquarium/Fishes` (`FishSpawner`) creates fish from it and keeps the list of all fish (`Fishes`): `Save` spawns the saved fish or, in a new game, one `GameplayConfig.starterSpecies` fish; each collected fish card spawns one more where it was clicked (`Spawn(species, xp, position)`). A prefab asset cannot reference scene objects, so `Spawn` calls `Fish.Initialize(species, xp, swimArea, feeder, drawOrder)` right after `Instantiate`. Species, `SwimArea` and `Feeder` are therefore plain private fields on `Fish`/`FishSwimmer`, not Inspector fields. A fish dragged into the scene by hand is not initialized and will not work. `FishSwimmer` swims in strokes: a stroke (`strokeSeconds`) eases the speed up to `strokeSpeed` (±`strokeSpeedVariation`) while `Sprite` squeezes to `1 − strokeSquashWidth` wide and `1 + strokeStretchHeight` tall and springs back; the fish then glides, losing speed to `glideDrag`, and strokes again below `nextStrokeSpeedRatio` of the top speed. Each stroke picks a heading at random (`turnChance` to turn around, up to `maxStrokeAngle` up or down, back inside within `edgeMargin` of the swim area's edges). A baby with food in the water steers at the nearest pellet every frame instead and strokes at `chaseSpeed`. These values are on the `Fish` prefab; spawned fish are copies, so edits on the prefab during Play do not reach fish already in the tank.
- `Feeder` drops a `FoodPellet` (`Assets/Prefabs/FoodPellet.prefab`) where the player clicks inside the swim area. Pellets sink to the bottom of the area and expire. A baby fish chases the nearest pellet; eating adds `GameplayConfig.foodXp` to its XP. Adults ignore food. `Feeder` ignores clicks on HUD elements: it picks the `HUD` panel under the pointer (`hudPanelRenderer`), so full-screen HUD layers must have `picking-mode="Ignore"`. The one exception is `pack-layer`: `CardPackView` makes it pickable while fish cards are out, so no food drops until every card is collected. `ShopView` also disables `Feeder` while the shop is open.
- Each fish species is a `FishSpecies` asset in `Assets/Data/Fish/` that holds only its identity and look (name, `FishRarity`, baby and adult sprite). A fish grows with XP: its `xp` starts at 0, rises by `GameplayConfig.xpPerSecond` every second and by `foodXp` per pellet, until it reaches its species' `adultXp`; meanwhile the fish grows from `babySize` to full size, then switches from the baby to the adult sprite. Every fish sprite is imported 0.9 units wide (adult size at scale 1), which fits the swim area.
- `Assets/Data/GameplayConfig.asset` is the single config, grouped under Money, Food, Rarities and Fish headers: starting money, card price, fish per card, income interval, food XP, a `RarityTier` per rarity (UI display color and card drop weight), the starter species, XP per second and the `fishes` list. Each `FishBalance` row in `fishes` holds one species' baby and adult income per second and the XP it needs to become an adult (`adultXp`); the card gives exactly the species in this list, and the starter species must be one of them. `Fish` looks its row up with `GetBalance` on every use, so edits during Play apply to fish already in the tank.
- `Wallet` (`MoneyWallet`) holds the money as a `double`, because balances reach the trillions. Code adds money with `Add` and spends it with `TrySpend` (false when the balance is too low); both raise `BalanceChanged`. `MoneyFormat` writes amounts like the reference game: `$12,500.00` below a million, `$72.556T` above.
- `Income` (`FishIncome`) pays every fish in `FishSpawner.Fishes` into the wallet once per `GameplayConfig.incomeIntervalSeconds`. A fish earns `Fish.IncomePerSecond` (baby or adult income of its species).
- `Save` (`GameSave`) keeps the game in `GameSave.SavePath` (`~/Library/Application Support/DefaultCompany/chillquarium/save.json`) as JSON (`SaveData`): the money, the number of unopened cards (`packs`) and each fish's species asset name and XP. Fish cards laid out but not clicked yet are saved as new baby fish, so quitting mid-card loses nothing. It loads at `Start`, autosaves every `autosaveIntervalSeconds` and saves on quit (also when Play Mode stops). Once a save exists, `startingMoney` and `starterSpecies` no longer apply; **Tools > Chillquarium > Delete Save** starts a new game (greyed out in Play Mode). Renaming a species asset loses saved fish of that species. Pellets and fish positions are not saved.
- `Sound` (an `AudioSource`) plays one-shot effects: `Feeder` plays `Drop` and `Eat`, `CardPackView` plays `Reveal` when a card opens and `Collect` when a fish card is clicked. The clips in `Assets/Audio/` are made from sine waves by `Tools/agent/generate-sounds.py` (no third-party sounds); rerun it to change them.
- `Aquarium/Bubbles` is a particle system: bubbles rise from the sand using the `Bubble` sprite (texture sheet in Sprites mode, `Sprite-Lit-Default` material, sorting order −5, behind the fish).
- `HUD` is the screen UI: a `PanelRenderer` with `Assets/UI/HUD.uxml` (structure), `HUD.uss` (style) and `HUDPanelSettings.asset` (scales with screen height from 1920×1080). Views that read the UI, like `MoneyView` and `ShopView`, sit on the `HUD` object so the project validator checks their element names against the UXML. `ShopView` opens the shop window from the Shop button; its Buy button spends the card price (disabled while the balance is too low), closes the shop and adds a pack to `CardPackView`. Bought cards wait as one pack button with a count (`x3`) on the sand below the swim area, so it never covers the water. Each click on it opens one card and adds its `GameplayConfig.fishPerCard` fish cards to the row, even while earlier cards are still out. The row stays one centered line across the screen and does not scroll; cards keep their size (`flex-shrink: 0`), so when more than about 10 are out they run off both screen edges and slide back in as others are collected. Each new card grows in from 0 width and rises into place, one after another (`cardPopStaggerMilliseconds`); the cards already out squeeze sideways for `cardSqueezeMilliseconds` and spring back. Fish cards sit above the pack, are built in C# from `...ClassName` fields, are framed by `Assets/UI/Images/CardFrame.png` and show each fish's name and rarity (in its color). Pressing a fish card, or passing over it with the left button held (one swipe collects many), collects it: the card spins away while its width shrinks to 0, so the remaining cards slide together and stay centered, and its fish spawns as a baby where the pointer was. `CardPackView.Update` polls the mouse and picks the panel along the swipe every `swipeSampleSpacing` pixels, because UI pointer-enter events do not arrive while the button is held. `CardRoll.PickSpecies` picks each fish: each rarity's drop weight is shared by the card species of that rarity. The pack layer is first in the UXML, so the top bar and the shop window draw on top of it and stay clickable while it blocks the water. Small animations are USS transitions started by toggling a class one frame later: `MoneyView` bumps the money panel and floats a "+$…" label on every gain, and `CardPackView` pops fish cards in and spins them away.
- Game scripts live in the `Chillquarium.Gameplay` namespace; editor tools live in `Chillquarium.EditorTools` under `Assets/Editor/`.

### Folder structure

| Folder | Contents |
|---|---|
| `Assets/Scripts/<Feature>/` | Scripts grouped by feature; scripts shared by all features, like `GameplayConfig.cs`, sit directly in `Assets/Scripts/` |
| `Assets/Data/` | The project's ScriptableObject assets |
| `Assets/Prefabs/` | Prefabs |
| `Assets/Sprites/` | Sprites cut from the generated art: `Fish/`, `Food/`, `Effects/`, `Background/`; `Placeholder/Square.png` is the flat square used by the background color bands |
| `Assets/Materials/`, `Assets/Shaders/` | Materials and shaders |
| `Assets/Audio/` | Audio files (generated by `Tools/agent/generate-sounds.py`) |
| `Assets/UI/` | UXML, USS, Panel Settings, fonts, themes and UI icons |
| `Assets/Scenes/` | Scenes; `Assets/Scenes/Dev/` is only for development and preview scenes |
| `Assets/Settings/` | Unity/URP project settings assets only; game data does not go here |
| `Assets/Editor/` | Editor-only tools, not included in builds (e.g. `ProjectValidator.cs`) |
| `Tools/agent/` | Helper scripts for agents (outside `Assets/`, so Unity does not import them) |
| `Art/` | Raw images generated with the Codex CLI, each with its `.prompt.txt` (outside `Assets/`). `SpriteSheetCutter` (in `Assets/Editor/`) cuts them into sprites and sets their import size |
| `docs/` | Project documents, e.g. `roadmap.md` (outside `Assets/`). `docs/references/` holds screenshots of the reference game for style |

- Put a new script in the matching feature folder; if none fits, create a new feature folder.

## Communication

- Reply in the language the user writes in. Keep sentences short, clear and simple.
- The user does not write code. The agent does all setup, including Inspector wiring.
- Do not ask the user code questions; when a technical decision is needed, explain the options in plain language and state your recommendation.

## Working style

- Keep changes small and focused on the request so they are easy to revert. Do not add unrequested improvements or refactors; ask before a larger change.
- At the end of each task, report:
  - which files changed,
  - which new Inspector fields were added (name and default value),
  - how to test it in Play Mode, in short steps.
- Keep this file up to date: when the folder structure or a rule changes, update `CLAUDE.md` in the same task.

## Unity scene and prefab dependencies

- Do not create or look up in code the dependencies that are visible in the Inspector and must be wired by hand. Declare them explicitly with `[SerializeField]` and wire them by drag and drop in the scene/prefab. This rule also applies to the operations the agent performs through the Unity Editor.
- If a required reference is not assigned, do not hide it with `Debug.Log`, an automatic `AddComponent`, a `GetComponent` assumption or a silent fix. Do not add runtime null checks, disable the component or throw a custom exception; a missing reference must produce a visible error at the point of use. Do not repeat the same checks in frequently called methods such as `Update()`.
- The `Debug.Log` ban only applies to hiding missing references. `Debug.Log` may be used when logging is needed for another purpose.
- The same rule applies to cameras: when a camera field is `null`, do not assign `Camera.main` or use any other fallback.
- Because `OnValidate()` can run before the Inspector setup is complete, it may only `return` on missing visual references. This is an editor setup exception; at runtime the reference is used directly.
- Code may only create game objects that genuinely need to be generated dynamically. Silently compensating for an Inspector dependency does not count as dynamic generation.
- Changing a serialized field's default value in code does not change the value already saved in scenes and prefabs. To change the value the game actually uses, change it in the scene or prefab.
- When a change should apply to every instance, edit the prefab asset itself. Use scene instance overrides only on purpose.
- When a serialized field is renamed, temporarily add `[FormerlySerializedAs("oldName")]` so scene/prefab values are not lost. Remove the attribute once the affected scenes and prefabs have been saved with the new name.

## Static visual surfaces

- Do not generate a `Mesh` at runtime for decorative backgrounds or other static flat surfaces. Use a sprite, Unity's built-in `Quad` mesh or an explicit mesh asset in the project.
- The size, position, renderer and material assignment of these surfaces are defined explicitly in the scene/prefab. For example, a fixed background is placed in the scene at a scale that covers the camera.
- Runtime mesh generation is only allowed when the geometry is truly dynamic: when vertices/topology change during gameplay or no suitable static mesh exists.

## Visuals and shaders

- All art (sprites, backgrounds, UI images) is generated in-house with the Codex CLI in the reference game's style. Do not import third-party asset packs.
- How the art was made: run `codex exec -s read-only --skip-git-repo-check -C <empty scratch folder> -i <reference images> - < prompt.txt` outside the repo, so the Codex agent cannot edit project files; the image lands in `~/.codex/generated_images/<session>/`. Ask for a transparent background and check it yourself (alpha count): Codex sometimes reports a transparent result as failed. Put several items in one sheet for a consistent style, save the raw image and prompt in `Art/`, then run `SpriteSheetCutter.Cut` and `ImportAsSprite`.
- The project uses URP. New shaders are written in HLSL for URP (`HLSLPROGRAM`, `"RenderPipeline" = "UniversalPipeline"`, URP `Core.hlsl`). Do not use the built-in pipeline's `CGPROGRAM` / `UnityCG.cginc`.
- Use `MaterialPropertyBlock` to give objects that share a material different per-object colors or values. Do not create material copies through `renderer.material` for this.

## UI development

- For Unity 6000.6 runtime UI, use `Panel Renderer + UXML + USS + Panel Settings`; do not use `UIDocument` for new UI.
- Link USS files through UXML/UI Builder; put UI behavior in C# scripts.
- Access UI elements through the root passed by `PanelRenderer.RegisterUIReloadCallback`. `PanelRenderer` has no `rootVisualElement`.
- Views keep UXML element names in `[SerializeField] string` fields whose names end with `Name` (`...ButtonName` for a `Button`, `...LabelName` for a `Label`); USS class names end with `ClassName`. The project validator checks these names against the view's UXML, so keep this naming.
- References: [Panel Renderer](https://docs.unity3d.com/6000.6/Documentation/Manual/ui-systems/panel-renderer-component.html), [Adding USS to UXML](https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-add-style-to-uxml.html), [Panel Settings](https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-Runtime-Panel-Settings.html).

## Input and Unity 6 API

- The project is set to the new Input System only (Active Input Handling: Input System Package). The legacy Input Manager API (`Input.GetKey`, `Input.mousePosition`, etc.) throws at runtime; use `UnityEngine.InputSystem` (`Mouse.current`, `Keyboard.current`, `InputAction`). The project's actions are in `Assets/Settings/InputSystem_Actions.inputactions`.
- Do not use APIs that are obsolete in Unity 6. Common cases:

| Old | New |
|---|---|
| `Rigidbody2D.velocity` | `Rigidbody2D.linearVelocity` |
| `Rigidbody2D.drag` | `Rigidbody2D.linearDamping` |
| `Rigidbody2D.angularDrag` | `Rigidbody2D.angularDamping` |
| `Rigidbody2D.isKinematic` | `Rigidbody2D.bodyType` |
| `FindObjectOfType` | `FindAnyObjectByType` / `FindFirstObjectByType` |
| `Physics2D.OverlapCircleNonAlloc` etc. | The `Physics2D.OverlapCircle` overload that takes a `ContactFilter2D` and a `List<Collider2D>` |

- If you are unsure whether an API exists or what its current name is, check it with reflection in the installed Unity version before using it.

## Unity CLI usage

- Unity Editor operations are done with the `unity command` CLI: scene, GameObject, component and Inspector edits, compilation, asset import and reading the Console.
- Do not hand-edit scene or prefab files as raw text; make changes through the Unity Editor.
- For fields that `component set_properties` cannot assign, such as structs or arrays, use `SerializedObject` inside `script eval`.
- Before editing a scene, check that the Editor is not in Play Mode (`EditorApplication.isPlayingOrWillChangePlaymode`). Scene changes made in Play Mode are lost.
- After editing a scene, save it (`EditorSceneManager.SaveScene`); unsaved scene changes are lost.
- A `script eval` call may have completed even if it timed out. Check the result on the file system or in the Editor before running it again.
- `AssetDatabase.SaveAssets` can re-save unrelated assets (e.g. files in `Assets/Settings/`). Check with `git status` after the work and revert unwanted changes.
- After a change, verify it with `Tools/agent/unity-check.py` (see Verification).

## Asset operations

- Move, rename and delete assets with `AssetDatabase.MoveAsset` / `RenameAsset` / `DeleteAsset`. This keeps `.meta` files and GUID references intact.
- Never use `rm -rf` in project folders.
- Do not save screenshots or temporary output inside `Assets/`; Unity imports them as assets.
- Before deleting an asset, check that it is not referenced by a scene, prefab or project setting.

## C# code style

- Classes are `sealed` unless they are meant to be inherited.
- In MonoBehaviours and ScriptableObjects, Inspector fields are `[SerializeField] private` and named in camelCase without a `_` prefix (e.g. `swimSpeed`). Expose them to other classes through properties when needed, not public fields.
- Gameplay tuning values (speed, duration, volume, counts, etc.) are not hard-coded constants; they are serialized Inspector fields so the user can tune the game without code.
- Keep game balance values in a single config ScriptableObject under `Assets/Data/` instead of spreading them across components. This includes per-species values such as incomes and adult XP. Visual and audio tuning stays on the component that uses it.
- Limit tuning fields with `[Min]` or `[Range]` (e.g. `[SerializeField, Min(0.01f)]`, `[SerializeField, Range(0f, 1f)]`).
- Comments are in English and explain why, not what. Only comment where the reason is not obvious from the code.

## C# member and method order

- Class members are defined in this order: Fields (serialized and private fields) ➔ Properties ➔ Unity lifecycle methods ➔ Public methods ➔ Private helper methods.
- Lifecycle methods called automatically by Unity (`Awake`, `OnEnable`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnDisable`, `OnDestroy`, `OnValidate`, etc.) sit at the top of the methods block.
- When there are several lifecycle methods, keep Unity's logical call order; other public and private methods are defined below them.

## Git

- Commit and push only when the user asks.
- Each topic gets its own commit; do not put unrelated changes in the same commit.
- Commit messages are in English, short and in the imperative mood (e.g. `Add bubble particle effect`). Do not add co-author or "generated by" lines.
- Dynamic font atlas changes in `Assets/UI/Fonts/*-SDF.asset` files are generated automatically by Unity; do not commit them unless the user asks.

## Verification

- Do not add automated tests.
- Run `Tools/agent/unity-check.py mark` before changing the project and `check` after. Do not report a task as done while `check` shows problems. Revert any changed file that the task did not mean to change.
- `Tools/agent/code-guard.py` catches code patterns forbidden by these rules. It runs automatically after Claude Code edits (`.claude/settings.json` hook) and inside `check`. Fix the violation; add `// agent-guard: allow <reason>` to a line only for a real exception.
- `check` also runs the Unity project validator (Tools > Chillquarium > Validate Project), which reports unassigned or empty Inspector fields on game scripts, missing scripts, broken references, prefab overrides in scenes, view `...Name` fields missing from the UXML and `...ClassName` fields that no stylesheet of that UXML defines. Fix its errors; confirm that reported overrides are intentional.
- The agent does not run Play Mode tests; the user does the play testing. After `check` is clean, tell the user in short steps what to test in Play Mode and how.
