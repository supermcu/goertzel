﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoertzelFSKDecoder
{
    public class GoertzelDecoder
    {
        public List<int> TargetFreqs { get; set; }

        private Dictionary<int, int> TargetFreqsDict = new Dictionary<int, int>();

        public List<double> Sample { get; set; }
        public int SampleRate { get; set; }

        public List<GoertzelHelper> GoertzelHelpers { get; set; }

        public List<double> FreqPowerResult { get; set; }

        // internal var.s for calculation
        private int sampleCount = int.MinValue;
        private int targetFreqCount = int.MinValue;
        private double Q0 = 0;
        private double Q1 = 0;
        private double Q2 = 0;

        public GoertzelDecoder()
        {
            GoertzelHelpers = new List<GoertzelHelper>();
            Sample = new List<double>();
            TargetFreqs = new List<int>();
            FreqPowerResult = new List<double>();
        }
        private void CalculateInternalVars()
        {
            // calculate sample count 
            sampleCount = Sample.Count;
            targetFreqCount = TargetFreqs.Count;
        }

        public void RunGoertzel()
        {
            // calculate sampleCount 
            CalculateInternalVars();

            // add all frequencies to dictionary and calculate nessesary constans to prevent continious calculation of constants
            for (int i = 0; i < TargetFreqs.Count; i++)
            {
                TargetFreqsDict.Add(i, TargetFreqs[i]);
                GoertzelHelpers.Add(new GoertzelHelper(SampleRate, sampleCount, TargetFreqsDict[i]));
            }

            // check if dict elements is missing or not
            if (TargetFreqsDict.Count == 0)
            {
                throw new Exception("Cannot run goertzel. Be sure that you have set sample rate, target frequencies properties");
            }
            else
            {
                DecodeGoertzel();
            }
        }


        private void DecodeGoertzel()
        {
            // for every target freq run decoding algo. for it
            for (int i = 0; i < targetFreqCount; i++)
            {

                var helper = GoertzelHelpers[i];
                for (int j = 0; j < sampleCount; j++)
                {

                    Q0 = helper.coeff * Q1 - Q2 + Sample[j];
                    Q2 = Q1;
                    Q1 = Q0;
                }

                CalculatePower(i, helper);
                ResetGoertzel();
            }
        }

        Dictionary<int, double> list = new Dictionary<int,double>();
        private void CalculatePower(int i, GoertzelHelper helper)
        {
            // calculate the power of targer freq
            var real = Math.Pow((Q1 - Q2 * helper.cosine), 2);
            var imag = Math.Pow(Q2 * helper.sine, 2);
            var mag = Math.Sqrt(real + imag);
            list.Add(helper.targetFreq, mag);
            // set result to the freqpowerres
            FreqPowerResult.Add(mag);
        }

        private void ResetGoertzel()
        {
            Q0 = 0;
            Q1 = 0;
            Q2 = 0;
        }
    }
}