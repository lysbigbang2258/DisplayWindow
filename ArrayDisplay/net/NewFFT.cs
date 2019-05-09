// 2018071814:04

using System;
using System.Windows;
using NationalInstruments;
using NationalInstruments.Analysis.Dsp;

namespace ArrayDisplay.Net {
    public static class NewFFT {


        public static Point[] Start(float[] waveform, int length)
        {
            double[] magnitudes;
            double[] phases;
            double[] dstfrom = new double[length];
            for (int i = 0; i < dstfrom.Length; i++)
            {
                dstfrom[i] = waveform[i];
            }
            //傅里叶变换
            ComplexDouble[] fftValue = Transforms.RealFft(dstfrom);
            //waveform data size
            int datasize = length;
            //number of samples for FFT data
            int fftnumofSamples = datasize / 2;
            // Get the magnitudes and phases of FFT array..
            ComplexDouble.DecomposeArrayPolar(fftValue, out magnitudes, out phases);
            double[] xwaveform = new double[fftnumofSamples];


            //            double deltaFreq = samplingRateNumericEdit.Value * scalingFactor;
            double scalingFactor = 1.0 / datasize;
            double deltaFreq = 31250 * scalingFactor; //采样率是31250 采样间隔
            double[] subsetOfMagnitudes = new double[fftnumofSamples];
            subsetOfMagnitudes[0] = magnitudes[0] * scalingFactor;
            Point[] resultPoints = new Point[fftnumofSamples];
            double[] logMagnitudes = new double[fftnumofSamples];
            double[] subsetOfPhases = new double[fftnumofSamples];
            for (int i = 1; i < fftnumofSamples; i++)
            {
                // Generating xwaveform with respect to which magnitude and phase will be plotted.
                xwaveform[i] = deltaFreq * i;
                subsetOfMagnitudes[i] = magnitudes[i] * scalingFactor * Math.Sqrt(2.0); // Storing only half the magnitudes array.
                subsetOfPhases[i] = phases[i]; // Storing only half of the phases array.

            }
            for (int i = 0; i < fftnumofSamples; i++)
            {
                if (Math.Abs(magnitudes[i]) < double.Epsilon)
                {
                    logMagnitudes[i] = 0;
                }
                else
                {
                    logMagnitudes[i] = 20.0 * Math.Log10(magnitudes[i]);
                }
                resultPoints[i] = new Point(xwaveform[i], logMagnitudes[i]);
            }

            return resultPoints;
        }
    }
}
