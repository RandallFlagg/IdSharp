namespace IdSharp.AudioInfo.Mpeg.Inspection;

internal sealed class PresetGuessRow
{
    public LameVersionGroup[] Vgs { get; set; } = new LameVersionGroup[3];
    public byte[] Tvs { get; set; } = new byte[7];
    public LamePreset Res { get; set; }

    public PresetGuessRow(
    byte tv1, byte tv2, byte tv3, byte tv4, byte tv5, byte tv6, byte tv7,
    LamePreset result,
    LameVersionGroup vg1,
    LameVersionGroup vg2 = LameVersionGroup.None,
    LameVersionGroup vg3 = LameVersionGroup.None)
    {
        Initialize(vg1, vg2, vg3, tv1, tv2, tv3, tv4, tv5, tv6, tv7, result);
    }

    public bool HasVersionGroup(LameVersionGroup vg1)
    {
        for (var i=0; i<3; i++)
        {
            if (vg1 == Vgs[i])
            {
                return true;
            }
        }

        return false;
    }

    private void Initialize(LameVersionGroup vg1, LameVersionGroup vg2, LameVersionGroup vg3,
        byte tv1, byte tv2, byte tv3, byte tv4, byte tv5, byte tv6, byte tv7,
        LamePreset result)
    {
        Vgs[0] = vg1;
        Vgs[1] = vg2;
        Vgs[2] = vg3;
        Tvs[0] = tv1;
        Tvs[1] = tv2;
        Tvs[2] = tv3;
        Tvs[3] = tv4;
        Tvs[4] = tv5;
        Tvs[5] = tv6;
        Tvs[6] = tv7;
        Res = result;
    }
}
