#!/usr/bin/env python3
"""Generates the game's sound effects from sine waves. Usage: generate-sounds.py Assets/Audio

Drop.wav    a soft water plip when food is dropped
Eat.wav     a short bubbly pop when a fish eats a pellet
Reveal.wav  a bright three-note chime when a fish card is opened
"""
import math, struct, sys, wave
from pathlib import Path

SAMPLE_RATE = 44100


def sweep(duration, start_freq, end_freq, decay, amplitude, start=0.0, length=None):
    """A sine tone whose pitch glides from start_freq to end_freq and fades out exponentially."""
    total = length or duration + start
    output = [0.0] * int(total * SAMPLE_RATE)
    phase = 0.0
    begin = int(start * SAMPLE_RATE)
    for index in range(begin, min(len(output), begin + int(duration * SAMPLE_RATE))):
        time = (index - begin) / SAMPLE_RATE
        progress = time / duration
        freq = start_freq * (end_freq / start_freq) ** progress
        phase += 2 * math.pi * freq / SAMPLE_RATE
        # A 3 ms fade-in keeps the start from clicking.
        attack = min(1.0, time / 0.003)
        output[index] += amplitude * attack * math.exp(-time / decay) * math.sin(phase)
    return output


def mix(*tracks):
    length = max(len(track) for track in tracks)
    return [sum(track[index] for track in tracks if index < len(track)) for index in range(length)]


def write(path, samples):
    peak = max(abs(sample) for sample in samples) or 1
    scale = 0.8 / peak
    with wave.open(str(path), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(SAMPLE_RATE)
        output.writeframes(b"".join(struct.pack("<h", int(sample * scale * 32767)) for sample in samples))


def main():
    folder = Path(sys.argv[1] if len(sys.argv) > 1 else "Assets/Audio")
    folder.mkdir(parents=True, exist_ok=True)

    write(folder / "Drop.wav", sweep(0.12, 900, 1500, 0.04, 1.0))
    write(folder / "Eat.wav", mix(sweep(0.09, 520, 180, 0.03, 1.0), sweep(0.06, 1200, 700, 0.015, 0.4, start=0.02)))

    # C6, E6, G6 played one after another, each ringing out.
    notes = [1046.5, 1318.5, 1568.0]
    chime = [sweep(0.7, freq, freq, 0.22, 0.6, start=index * 0.09, length=1.0) for index, freq in enumerate(notes)]
    overtones = [sweep(0.4, freq * 2, freq * 2, 0.1, 0.15, start=index * 0.09, length=1.0) for index, freq in enumerate(notes)]
    write(folder / "Reveal.wav", mix(*chime, *overtones))

    print(f"Wrote Drop.wav, Eat.wav and Reveal.wav to {folder}")


if __name__ == "__main__":
    main()
