// Matrix for doubling red channel
ColorMatrix doubleRed = new ColorMatrix(new float[][] {
    new float[] {2, 0, 0, 0, 0},  // Red × 2
    new float[] {0, 1, 0, 0, 0},
    new float[] {0, 0, 1, 0, 0},
    new float[] {0, 0, 0, 1, 0},
    new float[] {0, 0, 0, 0, 1}
});

// Matrix for adding brightness
ColorMatrix addBrightness = new ColorMatrix(new float[][] {
    new float[] {1, 0, 0, 0, 0},
    new float[] {0, 1, 0, 0, 0},
    new float[] {0, 0, 1, 0, 0},
    new float[] {0, 0, 0, 1, 0},
    new float[] {0.2f, 0.2f, 0.2f, 0, 1}  // Add 0.2 to all
});