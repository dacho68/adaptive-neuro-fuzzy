﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ANFIS.membership;
using System.Collections.Generic;
using ANFIS.training;
using System.Diagnostics;
using System.Linq;
using ANFIS;
using ANFIS.rextractors;
using ANFIS.misc;

namespace utest
{
    [TestClass]
    public class testANFIS
    {
        [TestMethod]
        public void TestOptimization1()
        {
            Backprop bprop = new Backprop(1e-2);
            BatchBackprop bbprop = new BatchBackprop(1e-2);
            QProp qprop = new QProp();
            StochasticBatch sprop = new StochasticBatch(100, 1e-2);
            StochasticQprop sqprop = new StochasticQprop(100);

            int trainingSamples = 1000;
            double[][] x = new double[trainingSamples][];
            double[][] y = new double[trainingSamples][];
            double[][] tx = new double[trainingSamples][];
            double[][] ty = new double[trainingSamples][];

            Random rnd = new Random();

            for (int i = 0; i < trainingSamples; i++)
            {

                double valx = 0.5 - rnd.NextDouble();
                double valy = 0.5 - rnd.NextDouble();

                x[i] = new double[] { valx, valy };
                y[i] = new double[] { 1 };


                valx = 0.5 - rnd.NextDouble();
                valy = 0.5 - rnd.NextDouble();

                tx[i] = new double[] { valx, valy };
                ty[i] = new double[] { 1 };
            }

            subTestOptimization1(bprop, x, y, tx, ty);
            subTestOptimization1(bbprop, x, y, tx, ty);
            subTestOptimization1(qprop, x, y, tx, ty);
            subTestOptimization1(sprop, x, y, tx, ty);
            subTestOptimization1(sqprop, x, y, tx, ty);

        }

        private static void subTestOptimization1(ITraining bprop, double[][] x, double[][] y, double[][] tx, double[][] ty)
        {
            GaussianRule2[] terms = new GaussianRule2[] { new GaussianRule2()};
            terms[0].Init(
                new double[] { 0.5, 0.3 },
                new double[] { 0 },
                new double[] { 0.0, 0.0 });

            int epoch = 0;
            int maxit = 1000;
            double trnError = 0.0;
            double tstError = 0.0;

            do
            {
                trnError = bprop.Iteration(x, y, terms);
                tstError = bprop.Error(tx, ty, terms);
            } while (!bprop.isTrainingstoped() && epoch++ < maxit);

            Trace.WriteLine(string.Format("Epochs {0} - Error {1}/{2}", epoch, trnError, tstError), "training");
            Assert.IsFalse(tstError > 1e-2);
            Assert.IsFalse(trnError > 1e-2);
            Assert.AreEqual(terms[0].Z[0], 1.0, 1e-2);
        }

        [TestMethod]
        public void TestOptimization2()
        {
            Backprop bprop = new Backprop(1e-2);
            BatchBackprop bbprop = new BatchBackprop(1e-2);
            QProp qprop = new QProp();
            StochasticBatch sprop = new StochasticBatch(100, 1e-2);
            StochasticQprop sqprop = new StochasticQprop(100);

            int trainingSamples = 100;
            double[][] x = new double[trainingSamples][];
            double[][] y = new double[trainingSamples][];
            double[][] tx = new double[trainingSamples][];
            double[][] ty = new double[trainingSamples][];

            Random rnd = new Random();

            for (int i = 0; i < trainingSamples; i++)
            {
                bool isRigth = i % 2 == 0;
                double valx = (isRigth ? 1 : -1) + (0.5 - rnd.NextDouble());

                x[i] = new double[] { valx };
                y[i] = new double[] { isRigth ? 1 : 0, isRigth ? 0 : 1 };

                valx = (isRigth ? 1 : -1) + (0.5 - rnd.NextDouble());

                tx[i] = new double[] { valx };
                ty[i] = new double[] { isRigth ? 1 : 0, isRigth ? 0 : 1 };
            }

            subTestOptimization2(bprop, x, y, tx, ty);
            subTestOptimization2(bbprop, x, y, tx, ty);
            subTestOptimization2(qprop, x, y, tx, ty);
            subTestOptimization2(sprop, x, y, tx, ty);
            subTestOptimization2(sqprop, x, y, tx, ty);

        }

        private static void subTestOptimization2(ITraining bprop, double[][] x, double[][] y, double[][] tx, double[][] ty)
        {
            GaussianRule2[] terms = new GaussianRule2[] { 
                new GaussianRule2(),
                new GaussianRule2() };

            terms[0].Init(new double[] { 0.5 }, new double[2] { 1, 0 }, new double[] { 0.0 });
            terms[1].Init(new double[] { 0.0 }, new double[2] { 0, 1 }, new double[] { 0.5 });
            

            int epoch = 0;
            int maxit = 1000;
            double trnError = 0.0;
            double tstError = 0.0;

            do
            {
                trnError = bprop.Iteration(x, y, terms);
                tstError = bprop.Error(tx, ty, terms);
            } while (!bprop.isTrainingstoped() && epoch++ < maxit);

            Trace.WriteLine(string.Format("Epochs {0} - Error {1}/{2}", epoch, trnError, tstError), "training");
            Assert.IsFalse(tstError > 1e-2);
            Assert.IsFalse(trnError > 1e-2);
            Assert.AreEqual(terms[0].Z[0], 1.0, 1e-2);
            Assert.AreEqual(terms[0].Z[1], 0.0, 1e-2);
            Assert.AreEqual(terms[1].Z[0], 0.0, 1e-2);
            Assert.AreEqual(terms[1].Z[1], 1.0, 1e-2);
            Assert.IsTrue(terms[0].Parameters[0] > 0);
            Assert.IsTrue(terms[1].Parameters[0] < terms[0].Parameters[0]);
        }

        [TestMethod]
        public void TestRulesetGeneration()
        {
            int trainingSamples = 100;
            double[][] x = new double[trainingSamples][];
            double[][] y = new double[trainingSamples][];

            Random rnd = new Random();

            for (int i = 0; i < trainingSamples; i++)
            {
                bool isRigth = i % 2 == 0;
                double valx = (isRigth ? 1 : -1) + (0.5 - rnd.NextDouble());

                x[i] = new double[] { valx, valx };
                y[i] = new double[] { isRigth ? 1 : 0, isRigth ? 0 : 1 };
            }

            KMEANSExtractorIO extractor = new KMEANSExtractorIO(2);
            List<GaussianRule> ruleBase = RuleSetFactory<GaussianRule>.Build(x, y, extractor);

            if (ruleBase[0].Z[0] > 0.5)
            {
                Assert.AreEqual(ruleBase[0].Z[0], 1, 1e-2);
                Assert.AreEqual(ruleBase[0].Z[1], 0, 1e-2);
                Assert.AreEqual(ruleBase[1].Z[1], 1, 1e-2);
                Assert.AreEqual(ruleBase[1].Z[0], 0, 1e-2);
                Assert.AreEqual(ruleBase[0].Parameters[0], 1, 1e-1);
                Assert.AreEqual(ruleBase[0].Parameters[1], 1, 1e-1);
                Assert.AreEqual(ruleBase[1].Parameters[0], -1, 1e-1);
                Assert.AreEqual(ruleBase[1].Parameters[1], -1, 1e-1);
            }
            else
            {
                Assert.AreEqual(ruleBase[0].Z[1], 1, 1e-2);
                Assert.AreEqual(ruleBase[0].Z[0], 0, 1e-2);
                Assert.AreEqual(ruleBase[1].Z[0], 1, 1e-2);
                Assert.AreEqual(ruleBase[1].Z[1], 0, 1e-2);
                Assert.AreEqual(ruleBase[1].Parameters[0], 1, 1e-1);
                Assert.AreEqual(ruleBase[1].Parameters[1], 1, 1e-1);
                Assert.AreEqual(ruleBase[0].Parameters[0], -1, 1e-1);
                Assert.AreEqual(ruleBase[0].Parameters[1], -1, 1e-1);
            }

        }

        [TestMethod]
        public void TestLogisticMap()
        {
            int trainingSamples = 3000;
            double[][] x = new double[trainingSamples][];
            double[][] y = new double[trainingSamples][];
            double[][] tx = new double[trainingSamples][];
            double[][] ty = new double[trainingSamples][];

            double px = 0.1;
            double r = 3.8;//3.56995;
            double lx = r * px * (1 - px);

            for (int i = 0; i < trainingSamples; i++)
            {
                x[i] = new double[] { px, lx };
                px = lx;
                lx = r * lx * (1 - lx);
                y[i] = new double[] { lx };
            }

            for (int i = 0; i < trainingSamples; i++)
            {
                tx[i] = new double[] { px, lx };
                px = lx;
                lx = r * lx * (1 - lx);
                ty[i] = new double[] { lx };
            }

            Backprop bprop = new Backprop(1e-2);
            bprop.AddRule += AddRule;
            BatchBackprop bbprop = new BatchBackprop(1e-2);
            bbprop.AddRule += AddRule;
            QProp qprop = new QProp();
            qprop.AddRule += AddRule;
            StochasticBatch sprop = new StochasticBatch(200, 1e-2);
            sprop.AddRule += AddRule;
            StochasticQprop sqprop = new StochasticQprop(200);
            sqprop.AddRule += AddRule;

            subtestLogisticsMap(x, y, tx, ty, bprop);
            subtestLogisticsMap(x, y, tx, ty, bbprop);
            subtestLogisticsMap(x, y, tx, ty, qprop);
            subtestLogisticsMap(x, y, tx, ty, sprop);
            subtestLogisticsMap(x, y, tx, ty, sqprop);
        }

        [TestMethod]
        public void TestIrisDataset()
        {
            int trainingSamples = IrisDataset.input.Length;

            Backprop bprop = new Backprop(1e-2, abstol: 1e-4, reltol: 1e-7, adjustThreshold: 1e-20);
            bprop.AddRule += AddRule;
            BatchBackprop bbprop = new BatchBackprop(1e-2, abstol: 1e-4, reltol: 1e-7, adjustThreshold: 1e-20);
            bbprop.AddRule += AddRule;
            QProp qprop = new QProp(abstol: 1e-4, reltol: 1e-7, adjustThreshold: 1e-20, InitialLearningRate: 1e-4);
            qprop.AddRule += AddRule;
            StochasticBatch sprop = new StochasticBatch(40, 1e-2);
            sprop.AddRule += AddRule;
            StochasticQprop sqprop = new StochasticQprop(40);
            sqprop.AddRule += AddRule;

            double[][] x;
            double[][] y;
            double[][] tx;
            double[][] ty;
            SampleData(IrisDataset.input, IrisDataset.output, 120, out x, out y, out tx, out ty);

            subtestIris(x, y, tx, ty, bprop);
            subtestIris(x, y, tx, ty, bbprop);
            subtestIris(x, y, tx, ty, qprop);
            subtestIris(x, y, tx, ty, sprop);
            subtestIris(x, y, tx, ty, sqprop);
        }

        private void SampleData(double[][] input, double[][] output, int TrainingSetSize, out double[][] x,out double[][] y, out double[][] tx, out double[][] ty)
        {
            var seq = input.Select((z, i) => i).ToArray();
            seq.Shuffle();

            x = new double[TrainingSetSize][];
            y = new double[TrainingSetSize][];
            tx = new double[input.Length - TrainingSetSize][];
            ty = new double[input.Length - TrainingSetSize][];

            int count = Math.Min(seq.Length, TrainingSetSize);
            for (int i = 0; i < count; i++)
            {
                x[i] = input[seq[i]];
                y[i] = output[seq[i]];
            }

            for (int i = count; i < input.Length; i++)
            {
                tx[i - count] = input[seq[i]];
                ty[i - count] = output[seq[i]];
            }

        }

        void AddRule(IList<IRule> RuleBase, double[] centroid, double[] consequence, double[] neighbourhood)
        {
            GaussianRule2 rule = new GaussianRule2();
            rule.Init(centroid, consequence, neighbourhood);
            RuleBase.Add(rule);
        }

        private static void subtestLogisticsMap(double[][] x, double[][] y, double[][] tx, double[][] ty, ITraining bprop)
        {
            KMEANSExtractorIO extractor = new KMEANSExtractorIO(10);
            var timer = Stopwatch.StartNew();
            ANFIS.ANFIS fis = ANFISFActory<GaussianRule2>.Build(x, y, extractor, bprop, 1000);
            timer.Stop();

            double err = bprop.Error(tx, ty, fis.RuleBase);

            Trace.WriteLine(string.Format("[{1}]\tLogistic map Error {0}\tElapsed {2}\tRuleBase {3}", err, bprop.GetType().Name, timer.Elapsed, fis.RuleBase.Length), "training");
            Assert.IsFalse(err > 1e-2);
        }

        private static void subtestIris(double[][] x, double[][] y, double[][] tx, double[][] ty, ITraining bprop)
        {
            KMEANSExtractorI extractor = new KMEANSExtractorI(15);
            var timer = Stopwatch.StartNew();
            ANFIS.ANFIS fis = ANFISFActory<GaussianRule2>.Build(x, y, extractor, bprop, 1000);
            timer.Stop();

            double err = bprop.Error(tx, ty, fis.RuleBase);

            double correctClass = 0;
            for (int i = 0; i < tx.Length; i++)
            {
                double[] o = fis.Inference(tx[i]);
                for (int j = 0; j < ty[i].Length; j++)
                    if (ty[i][j] == 1.0 && o[j] == o.Max())
                        correctClass++;
            }

            Trace.WriteLine(string.Format("[{1}]\tIris Dataset Error {0} Classification Error {4}\tElapsed {2}\tRuleBase {3}", err, bprop.GetType().Name, timer.Elapsed, fis.RuleBase.Length, 1.0 - correctClass / ty.Length), "training");
            Assert.IsFalse(ty.Length - correctClass > 2);
        }
    }
}
