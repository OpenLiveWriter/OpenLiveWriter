#!/usr/bin/env python3
"""Section-by-section ribbon comparison for Open Live Writer QA.

Takes two full-screen captures (old reference build and new build), crops
named ribbon sections from each using measured boundaries, and writes:

  compare.png   contact sheet: one row per section, old crop above new crop
  report.txt    per-section widths, background colors, pixel diff, and an
                overlap check (rightmost ink vs section right edge)

Usage: ribbon_compare.py old.png new.png outdir
"""

import sys
from collections import Counter

from PIL import Image

# Ribbon band in native capture pixels (capture is 2x logical DPI).
BAND_TOP = 60
BAND_BOTTOM = 300

# Measured section boundaries (native px) for the two builds at a maximized
# 1512x820 logical window. Adjust if the window geometry changes.
OLD_SECTIONS = [
    ("clipboard", 0, 215),
    ("publish", 215, 565),
    ("font", 565, 1005),
    ("paragraph", 1005, 1215),
    ("styles", 1215, 2030),
    ("insert", 2030, 2340),
    ("editing", 2340, 2620),
]
NEW_SECTIONS = [
    ("clipboard", 0, 240),
    ("publish", 240, 610),
    ("font", 610, 1078),
    ("paragraph", 1078, 1228),
    ("style", 1228, 2022),
    ("insert", 2022, 2330),
    ("editing", 2330, 2534),
]

# Compare these old<->new section pairs by name.
PAIRS = [
    ("clipboard", "clipboard"),
    ("publish", "publish"),
    ("font", "font"),
    ("paragraph", "paragraph"),
    ("styles", "style"),
    ("insert", "insert"),
    ("editing", "editing"),
]


def luminance(px):
    return (px[0] * 299 + px[1] * 587 + px[2] * 114) // 1000


def bg_color(img, box):
    crop = img.crop(box)
    return Counter(crop.getdata()).most_common(1)[0][0]


def rightmost_ink(img, box, bg_lum=230):
    x0, x1 = box[0], box[2]
    for x in range(x1 - 1, x0, -1):
        for y in range(box[1], box[3], 2):
            if luminance(img.getpixel((x, y))) < bg_lum:
                return x
    return None


def main():
    old_path, new_path, outdir = sys.argv[1], sys.argv[2], sys.argv[3]
    old = Image.open(old_path).convert("RGB")
    new = Image.open(new_path).convert("RGB")
    old_map = {n: (a, b) for n, a, b in OLD_SECTIONS}
    new_map = {n: (a, b) for n, a, b in NEW_SECTIONS}

    lines = []
    rows = []
    for old_name, new_name in PAIRS:
        o = old_map.get(old_name)
        n = new_map.get(new_name)
        title = "%s vs %s" % (old_name, new_name)
        lines.append("--- %s ---" % title)
        for label, img, sec in (("old", old, o), ("new", new, n)):
            if not sec:
                lines.append("%s: MISSING" % label)
                continue
            box = (sec[0], BAND_TOP, sec[1], BAND_BOTTOM)
            bg = bg_color(img, box)
            ink = rightmost_ink(img, box)
            gap = (sec[1] - ink) if ink is not None else None
            flag = ""
            if gap is not None and gap < 4:
                flag = "  <-- RIGHT EDGE OVERLAP (gap=%s)" % gap
            lines.append(
                "%s: w=%d bg=%s right_gap=%s%s" % (label, sec[1] - sec[0], bg, gap, flag))
        if o and n:
            w = min(o[1] - o[0], n[1] - n[0])
            diff = 0
            count = 0
            for x in range(w):
                for y in range(BAND_TOP, BAND_BOTTOM, 3):
                    po = old.getpixel((o[0] + x, y))
                    pn = new.getpixel((n[0] + x, y))
                    diff += abs(po[0] - pn[0]) + abs(po[1] - pn[1]) + abs(po[2] - pn[2])
                    count += 1
            lines.append("widths: old=%d new=%d (delta %+d); mean abs diff %.1f"
                         % (o[1] - o[0], n[1] - n[0], (n[1] - n[0]) - (o[1] - o[0]), diff / max(count, 1)))
        crops = []
        for img, sec in ((old, o), (new, n)):
            if sec:
                crops.append(img.crop((sec[0], BAND_TOP, sec[1], BAND_BOTTOM)))
        if crops:
            mw = max(c.width for c in crops)
            sh = sum(c.height for c in crops) + 8 * len(crops)
            strip = Image.new("RGB", (mw, sh), (255, 0, 0))
            yy = 0
            for c in crops:
                strip.paste(c, (0, yy))
                yy += c.height + 8
            rows.append((title, strip))

    if rows:
        cw = max(s.width for _, s in rows)
        ch = sum(s.height + 26 for _, s in rows)
        sheet = Image.new("RGB", (cw, ch), (255, 255, 255))
        yy = 0
        for title, strip in rows:
            sheet.paste(strip, (0, yy + 22))
            yy += strip.height + 26
        sheet.save("%s/compare.png" % outdir)

    report = "\n".join(lines)
    with open("%s/report.txt" % outdir, "w") as f:
        f.write(report + "\n")
    print(report)


if __name__ == "__main__":
    main()
