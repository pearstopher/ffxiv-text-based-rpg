"""Generate images/icon.png (512x512): a chat bubble with text lines and a cursor on a dark tile.

Pure Python (no Pillow): shapes are tested per pixel with 4x4 supersampling for smooth edges.
Run from the repo root:  python -I images/make_icon.py
"""

import struct
import zlib
from pathlib import Path

SIZE = 512
SS = 4  # supersampling per axis

BG = (24, 26, 38)          # dark slate tile
BUBBLE = (236, 230, 214)   # parchment
INK = (52, 56, 78)         # text lines
CURSOR = (214, 160, 72)    # amber cursor


def in_round_rect(x, y, x0, y0, x1, y1, r):
    if x < x0 or x > x1 or y < y0 or y > y1:
        return False
    cx = min(max(x, x0 + r), x1 - r)
    cy = min(max(y, y0 + r), y1 - r)
    return (x - cx) ** 2 + (y - cy) ** 2 <= r * r


def in_triangle(x, y, a, b, c):
    def side(p, q, r):
        return (p[0] - r[0]) * (q[1] - r[1]) - (q[0] - r[0]) * (p[1] - r[1])
    d1, d2, d3 = side((x, y), a, b), side((x, y), b, c), side((x, y), c, a)
    neg = d1 < 0 or d2 < 0 or d3 < 0
    pos = d1 > 0 or d2 > 0 or d3 > 0
    return not (neg and pos)


def colour_at(x, y):
    """Colour of the topmost shape at (x, y), or None outside the tile."""
    if not in_round_rect(x, y, 0, 0, SIZE, SIZE, 96):
        return None
    # cursor block after the last line
    if in_round_rect(x, y, 300, 268, 330, 312, 6):
        return CURSOR
    # text lines inside the bubble
    for (x0, x1, yc) in ((132, 380, 172), (132, 340, 222), (132, 282, 290)):
        if in_round_rect(x, y, x0, yc - 14, x1, yc + 14, 14):
            return INK
    # bubble body and tail
    if in_round_rect(x, y, 88, 104, 424, 352, 56) or in_triangle(x, y, (150, 330), (250, 340), (128, 430)):
        return BUBBLE
    return BG


def render():
    rows = []
    for py in range(SIZE):
        row = bytearray([0])  # PNG filter type 0
        for px in range(SIZE):
            r = g = b = a = 0
            for sy in range(SS):
                for sx in range(SS):
                    c = colour_at(px + (sx + 0.5) / SS, py + (sy + 0.5) / SS)
                    if c is not None:
                        r += c[0]; g += c[1]; b += c[2]; a += 255
            n = SS * SS
            if a:
                k = a / 255
                row += bytes((round(r / k), round(g / k), round(b / k), round(a / n)))
            else:
                row += b"\0\0\0\0"
        rows.append(bytes(row))
    return b"".join(rows)


def png(raw):
    def chunk(tag, data):
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)
    header = struct.pack(">IIBBBBB", SIZE, SIZE, 8, 6, 0, 0, 0)  # 8-bit RGBA
    return b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header) + chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")


if __name__ == "__main__":
    out = Path(__file__).with_name("icon.png")
    out.write_bytes(png(render()))
    print(f"wrote {out}")
