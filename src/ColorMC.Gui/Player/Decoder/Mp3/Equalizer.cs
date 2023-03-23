using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public sealed class Equalizer
{
    /**
     * Equalizer setting to denote that a given band will not be
     * present in the output signal.
     */
    public const float BAND_NOT_PRESENT = float.NegativeInfinity;

    private const int BANDS = 32;

    private readonly float[] settings = new float[BANDS];

    /**
     * Creates a new <code>Equalizer</code> instance.
     */
    public Equalizer()
    {
    }

    //	private Equalizer(float b1, float b2, float b3, float b4, float b5,
    //					 float b6, float b7, float b8, float b9, float b10, float b11,
    //					 float b12, float b13, float b14, float b15, float b16,
    //					 float b17, float b18, float b19, float b20);

    public void SetFrom(float[] eq)
    {
        Reset();
        int max = Math.Min(eq.Length, BANDS);

        for (int i = 0; i < max; i++)
        {
            settings[i] = Limit(eq[i]);
        }
    }

    public void SetFrom(EQFunction eq)
    {
        Reset();

        for (int i = 0; i < BANDS; i++)
        {
            settings[i] = Limit(eq.GetBand(i));
        }
    }

    /**
     * Sets the bands of this equalizer to the value the bands of
     * another equalizer. Bands that are not present in both equalizers are ignored.
     */
    public void SetFrom(Equalizer eq)
    {
        if (eq != this)
        {
            SetFrom(eq.settings);
        }
    }


    /**
     * Sets all bands to 0.0
     */
    public void Reset()
    {
        for (int i = 0; i < BANDS; i++)
        {
            settings[i] = 0.0f;
        }
    }


    private float Limit(float eq)
    {
        if (eq == BAND_NOT_PRESENT)
            return eq;
        if (eq > 1.0f)
            return 1.0f;
        return Math.Max(eq, -1.0f);

    }

    abstract public class EQFunction
    {
        /**
         * Returns the setting of a band in the equalizer.
         *
         * @param band The index of the band to retrieve the setting
         *             for.
         * @return the setting of the specified band. This is a value between
         * -1 and +1.
         */
        public float GetBand(int band)
        {
            return 0.0f;
        }

    }
}
