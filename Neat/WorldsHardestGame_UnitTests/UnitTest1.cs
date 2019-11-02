using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WorldsHardestGame_UnitTests
{
    using World_s_hardest_game;
    using Neuroevolution.Neat;
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void UnitTests_LineIntersection_Simple()
        {
            Game.Vector p1 = new Game.Vector(0, 2);
            Game.Vector p2 = new Game.Vector(2, 4);
            Game.Vector p3 = new Game.Vector(4, 0);
            Game.Vector p4 = new Game.Vector(0, 4);
            Game g = new Game();
            var res = g.LineIntersection(p1, p2, p3, p4);
            Assert.AreEqual(new Game.Vector(1, 3), res);

        }

        [TestMethod]
        public void UnitTests_LineIntersection()
        {
            Game.Vector p1 = new Game.Vector(2, 2);
            Game.Vector p2 = new Game.Vector(2, 0);
            Game.Vector p3 = new Game.Vector(0, 1);
            Game.Vector p4 = new Game.Vector(3, 1);
            Game g = new Game();
            var res = g.LineIntersection(p1, p2, p3, p4);
            Assert.AreEqual(new Game.Vector(2, 1), res);

        }

     //   [TestMethod]
     //   public void NeatUnitTests_CompatibilityCounting()
     //   {
     //       Node n1 = new Node(NodeType.Sensor, 0, 0);
     //       Node n2 = new Node(NodeType.Sensor, 0, 1);
     //       Node n3 = new Node(NodeType.Sensor, 0, 2);
     //       Node n4 = new Node(NodeType.Hidden, 1, 3);
     //       Node n5 = new Node(NodeType.Hidden, 1, 4);
     //       Node n6 = new Node(NodeType.Output, 2, 5);
     //       List<Node> nodesin1 = new List<Node>();
     //       nodesin1.Add(n1);
     //       nodesin1.Add(n2);
     //       nodesin1.Add(n3);
     //       List<Node> hidden1 = new List<Node> { n4 };
     //       List<Node> hidden2 = new List<Node> { n4, n5 };
     //       List<Node> output = new List<Node> { n6 };
     //       List<object> players = new List<object>();
     //       for (int i = 0; i < 100; i++)
     //       {
     //           players.Add(new object());
     //       }
     //       Gene gg1 = new Gene(n1, n4, 1f, GeneType.Enabled, 1);
     //       Gene gg2 = new Gene(n3, n4, 2f, GeneType.Enabled, 2);
     //       Gene gg3 = new Gene(n5, n4, 3f, GeneType.Enabled, 3);
     //       Gene gg4 = new Gene(n2, n5, 2.1f, GeneType.Enabled, 5);
     //       Gene gg5 = new Gene(n1, n5, 3.1f, GeneType.Enabled, 6);
     //       Gene ggD = new Gene(n2, n4, 50f, GeneType.Disabled, 4);
     //
     //       Gene ggD_ = new Gene(n2, n4, -20f, GeneType.Disabled, 4);
     //       Gene gg1_ = new Gene(n1, n4, 1.5f, GeneType.Enabled, 1);
     //       Gene gg2_ = new Gene(n3, n4, 2f, GeneType.Enabled, 2);
     //       Gene gg3_ = new Gene(n5, n4, 3.2f, GeneType.Enabled, 3);
     //       Gene gg4_ = new Gene(n2, n5, 2.2f, GeneType.Enabled, 5);
     //       Gene gg6_ = new Gene(n6, n5, 4f, GeneType.Enabled, 7);
     //       Gene gg7_ = new Gene(n3, n6, 5f, GeneType.Enabled, 8);
     //       SortedList<int, Gene> genes1 = new SortedList<int, Gene> { { 1, gg1 }, { 2, gg2 }, { 3, gg3 }, { 5, gg4 }, { 6, gg5 }, { 4, ggD } };
     //       SortedList<int, Gene> genes2 = new SortedList<int, Gene> { { 1, gg1_ }, { 2, gg2_ }, { 3, gg3_ }, { 5, gg4_ }, { 7, gg6_ }, { 8, gg7_ }, { 4, ggD_ } };
     //       var g1 = new Genome(nodesin1, hidden1, output, genes1, null);
     //       var g2 = new Genome(nodesin1, hidden2, output, genes2, null);
     //       Neat neat = new Neat(null, null, null, genes1, (o => 0f), players);
     //       float result = neat.CountCompatibility(g1, g2);
     //       Assert.AreEqual(true, (result > 2f/7 + 1f/7 + 0.8f * 0.4f - 0.111f && result < 2f / 7 + 1f / 7 + 0.8f * 0.4f + 0.111f));
     //   }

       // [TestMethod]
       // public void NeatUnitTests_CompatibilityCountingSameFitness()
       // {
       //     Node n1 = new Node(NodeType.Sensor, 0, 0);
       //     Node n2 = new Node(NodeType.Sensor, 0, 1);
       //     Node n3 = new Node(NodeType.Sensor, 0, 2);
       //     Node n4 = new Node(NodeType.Hidden, 1, 3);
       //     Node n5 = new Node(NodeType.Hidden, 1, 4);
       //     Node n6 = new Node(NodeType.Output, 2, 5);
       //     List<Node> nodesin1 = new List<Node>();
       //     nodesin1.Add(n1);
       //     nodesin1.Add(n2);
       //     nodesin1.Add(n3);
       //     List<Node> hidden1 = new List<Node> { n4 };
       //     List<Node> hidden2 = new List<Node> { n4, n5 };
       //     List<Node> output = new List<Node> { n6 };
       //     List<object> players = new List<object>();
       //     for (int i = 0; i < 100; i++)
       //     {
       //         players.Add(new object());
       //     }
       //     Gene gg1 = new Gene(n1, n4, 1f, GeneType.Enabled, 1);
       //     Gene gg2 = new Gene(n3, n4, 2f, GeneType.Enabled, 2);
       //     Gene gg3 = new Gene(n5, n4, 3f, GeneType.Enabled, 3);
       //     Gene gg4 = new Gene(n2, n5, 2.1f, GeneType.Enabled, 5);
       //     Gene gg5 = new Gene(n1, n5, 3.1f, GeneType.Enabled, 6);
       //     Gene ggD = new Gene(n2, n4, 50f, GeneType.Disabled, 4);
       //
       //     SortedList<int, Gene> genes1 = new SortedList<int, Gene> { { 1, gg1 }, { 2, gg2 }, { 3, gg3 }, { 5, gg4 }, { 6, gg5 }, { 4, ggD } };
       //     SortedList<int, Gene> genes2 = new SortedList<int, Gene>();
       //     foreach (var item in genes1)
       //     {
       //         genes2.Add(item.Value.Innovation, item.Value.ShallowCopy());
       //     }
       //     var g1 = new Genome(nodesin1, hidden1, output, genes1, null);
       //     var g2 = new Genome(nodesin1, hidden2, output, genes2, null);
       //     Neat neat = new Neat(null, null, null, genes1, (o => 0f), players);
       //     var result = g1.Crossover(g2, new Random(), null, true);
       //     bool hidden = (result.HiddenNodes.Count == g1.HiddenNodes.Count) ? true : false;
       //     Assert.AreEqual(false, hidden);
       // }
    }
}
