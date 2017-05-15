using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/

namespace Code4Life
{
    internal class Player
    {
        private const int MaxDataFiles = 3;
        private const int MaxMolecules = 10;

        private enum ModuleType
        {
            DIAGNOSIS, MOLECULES, LABORATORY
        }

        private enum MoleculeType
        {
            A, B, C, D, E
        }

        private enum Action { Goto, Connect }

        private class Module
        {
            public string Name { get; set; }
        }

        private class Samples : Module
        {
            public Samples()
            {
                Name = "SAMPLES";
            }
        }

        private class Diagnosis : Module
        {
            public List<SampleData> CloudData { get; set; }

            public Diagnosis()
            {
                Name = "DIAGNOSIS";
                CloudData = new List<SampleData>();
            }
        }

        private class Molecules : Module
        {
            public Molecules()
            {
                Name = "MOLECULES";
            }
        }

        private class Laboratory : Module
        {
            public Laboratory()
            {
                Name = "LABORATORY";
            }
        }

        private class SampleData
        {
            public int Id { get; set; }
            public int CarriedBy { get; set; }
            public Dictionary<MoleculeType, int> Cost { get; set; }
            public int Health { get; set; }
            public bool Diagnosed { get { return Health > -1; } }
            public Dictionary<MoleculeType, int> Paid { get; set; }

            public SampleData()
            {
                Paid = new Dictionary<MoleculeType, int>()
                {
                    {MoleculeType.A, 0}, {MoleculeType.B, 0}, {MoleculeType.C, 0}, {MoleculeType.D, 0}, {MoleculeType.E, 0}
                };
            }

            public bool IsPossible()
            {
                return myRobot.Storage.All(d => (Cost[d.Key] - (myRobot.Expertise[d.Key] + d.Value)) <= 0 || moleculesAvailable[d.Key] >= (Cost[d.Key] - (myRobot.Expertise[d.Key] + d.Value)));
            }

            public override string ToString()
            {
                if (Cost == null) return Id.ToString();
                else return Id + " " + Health + " " + string.Join(",", Cost);
            }

            public bool Dead { get; set; }
        }

        private class Robot
        {
            public int Health { get; set; }
            public Module Target { get; set; }
            public Dictionary<MoleculeType, int> Storage { get; set; }
            public Action Action { get; set; }
            public string TargetName { get; set; }
            public List<SampleData> SampleData { get; set; }

            public Robot()
            {
                Storage = new Dictionary<MoleculeType, int>();
                SampleData = new List<SampleData>();
            }

            public void PrintAction(string id)
            {
                if (Action == Player.Action.Goto)
                {
                    Console.WriteLine(string.Format("GOTO {0} {1}", Target.Name, message));
                }
                else
                {
                    Console.WriteLine(string.Format("CONNECT {0} {1}", id, message));
                }
            }

            public bool CanComplete(SampleData data)
            {
                if (!data.Diagnosed) return false;

                return Storage.All(d => d.Value >= data.Cost[d.Key] - Expertise[d.Key]);
            }

            internal void MoveTo(Module module)
            {
                Action = Action.Goto;
                Target = module;
            }

            internal void Connect(string rank)
            {
                Action = Player.Action.Connect;
                TargetName = rank;
            }

            public int Carrying { get; set; }

            public int Eta { get; set; }

            public Dictionary<MoleculeType, int> Expertise { get; set; }
        }

        private static Samples samples = new Samples();
        private static Diagnosis diagnosis = new Diagnosis();
        private static Molecules molecules = new Molecules();
        private static Laboratory laboratory = new Laboratory();

        private static Robot myRobot = new Robot();
        private static Dictionary<MoleculeType, int> moleculesAvailable = new Dictionary<MoleculeType, int>();
        private static List<SampleData> sampleData = new List<SampleData>();
        private static string message = "";

        private static void Main(string[] args)
        {
            var enemyRobot = new Robot();

            string[] inputs;
            int projectCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < projectCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int a = int.Parse(inputs[0]);
                int b = int.Parse(inputs[1]);
                int c = int.Parse(inputs[2]);
                int d = int.Parse(inputs[3]);
                int e = int.Parse(inputs[4]);
                Console.Error.WriteLine("Projects: " + string.Join(",", inputs));
                //sampleData.Add(new SampleData
                //{
                //    CarriedBy = 1,
                //    Cost = new Dictionary<MoleculeType, int>
                //            {
                //                {MoleculeType.A, a}, {MoleculeType.B, b}, {MoleculeType.C, c},
                //                {MoleculeType.D, d}, {MoleculeType.E, e}
                //            },
                //    Diagnosed = true,
                //    Health = 30
                //});
            }

            // game loop
            while (true)
            {
                sampleData.ForEach(d => d.Dead = true);

                for (int i = 0; i < 2; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    Console.Error.WriteLine("i=" + i + " " + string.Join(",", inputs));
                    string target = inputs[0];
                    int eta = int.Parse(inputs[1]);
                    int score = int.Parse(inputs[2]);
                    int storageA = int.Parse(inputs[3]);
                    int storageB = int.Parse(inputs[4]);
                    int storageC = int.Parse(inputs[5]);
                    int storageD = int.Parse(inputs[6]);
                    int storageE = int.Parse(inputs[7]);
                    int expertiseA = int.Parse(inputs[8]);
                    int expertiseB = int.Parse(inputs[9]);
                    int expertiseC = int.Parse(inputs[10]);
                    int expertiseD = int.Parse(inputs[11]);
                    int expertiseE = int.Parse(inputs[12]);
                    var robot = i == 0 ? myRobot : enemyRobot;
                    switch (target)
                    {
                        case "SAMPLES": robot.Target = samples; break;
                        case "DIAGNOSIS": robot.Target = diagnosis; break;
                        case "MOLECULES": robot.Target = molecules; break;
                        case "LABORATORY": robot.Target = laboratory; break;
                    }
                    robot.Eta = eta;
                    robot.Health = score;
                    robot.Storage = new Dictionary<MoleculeType, int>
                            {
                                {MoleculeType.A, storageA}, {MoleculeType.B, storageB}, {MoleculeType.C, storageC},
                                {MoleculeType.D, storageD}, {MoleculeType.E, storageE}
                            };
                    robot.Expertise = new Dictionary<MoleculeType, int>
                    {
                        {MoleculeType.A, expertiseA}, {MoleculeType.B, expertiseB}, {MoleculeType.C, expertiseC},
                        {MoleculeType.D, expertiseD}, {MoleculeType.E, expertiseE}
                    };
                }

                inputs = Console.ReadLine().Split(' ');
                int availableA = int.Parse(inputs[0]);
                int availableB = int.Parse(inputs[1]);
                int availableC = int.Parse(inputs[2]);
                int availableD = int.Parse(inputs[3]);
                int availableE = int.Parse(inputs[4]);
                moleculesAvailable = new Dictionary<MoleculeType, int>
                {
                    {MoleculeType.A, availableA}, {MoleculeType.B, availableB}, {MoleculeType.C, availableC},
                    {MoleculeType.D, availableD}, {MoleculeType.E, availableE}
                };

                int sampleCount = int.Parse(Console.ReadLine());
                for (int i = 0; i < sampleCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int sampleId = int.Parse(inputs[0]);
                    int carriedBy = int.Parse(inputs[1]);
                    int rank = int.Parse(inputs[2]);
                    string expertiseGain = inputs[3];
                    int health = int.Parse(inputs[4]);
                    int costA = int.Parse(inputs[5]);
                    int costB = int.Parse(inputs[6]);
                    int costC = int.Parse(inputs[7]);
                    int costD = int.Parse(inputs[8]);
                    int costE = int.Parse(inputs[9]);

                    var sample = sampleData.FirstOrDefault(d => d.Id == sampleId);
                    if (sample == null)
                    {
                        sample = new SampleData()
                        {
                            Id = sampleId
                        };
                        sampleData.Add(sample);
                    }
                    sample.Health = health;
                    sample.Cost = new Dictionary<MoleculeType, int>
                            {
                                {MoleculeType.A, costA}, {MoleculeType.B, costB}, {MoleculeType.C, costC},
                                {MoleculeType.D, costD}, {MoleculeType.E, costE}
                            };
                    sample.CarriedBy = carriedBy;
                    sample.Dead = false;
                }
                sampleData.RemoveAll(d => d.Dead /*&& d.Health != 30*/);

                diagnosis.CloudData.Clear();
                diagnosis.CloudData.AddRange(sampleData.Where(d => d.CarriedBy == -1));
                myRobot.SampleData.Clear();
                enemyRobot.SampleData.Clear();
                myRobot.SampleData.AddRange(sampleData.Where(d => d.CarriedBy == 0));
                enemyRobot.SampleData.AddRange(sampleData.Where(d => d.CarriedBy == 1));
                myRobot.Carrying = myRobot.SampleData.Count();
                enemyRobot.Carrying = enemyRobot.SampleData.Count();

                Console.Error.WriteLine("Samples: {0}", string.Join(",", diagnosis.CloudData));
                Console.Error.WriteLine("Carried Samples: {0}\n{1}", string.Join(",", myRobot.SampleData), string.Join(",", myRobot.SampleData.Select(d => d.Id + string.Join(",", d.Paid))));
                Console.Error.WriteLine("Storage: {0}", string.Join(",", myRobot.Storage));

                if (myRobot.Eta > 0)
                {
                    Console.WriteLine("WAIT " + message);
                    continue;
                }

                var canHoldMore = myRobot.Storage.Sum(d => d.Value) < MaxMolecules;
                var samplesCantComplete = myRobot.SampleData.FirstOrDefault(s => s.Paid.Any(d => d.Value <= (s.Cost[d.Key] - myRobot.Expertise[d.Key]) && moleculesAvailable[d.Key] > 0));
                if (myRobot.Target is Samples)
                {
                    var diagnosed = diagnosis.CloudData.Where(d => d.IsPossible());
                    if (diagnosed.Count() > 0 || myRobot.Carrying >= MaxDataFiles) //check if enemy is already there
                    {
                        message = diagnosed.Count() > 0 ? "Something at diag" : "holding max";
                        myRobot.MoveTo(diagnosis);
                    }
                    else
                    {
                        message = "grab sample";
                        string rank = "2";
                        myRobot.Connect(rank);
                    }
                }
                else if (myRobot.Target is Diagnosis)
                {
                    var diagnosed = diagnosis.CloudData.Where(d => d.IsPossible());
                    if (diagnosed.Count() > 0 && myRobot.Carrying < MaxDataFiles)
                    {
                        message = "Get cloud";
                        var dat = diagnosed.FirstOrDefault(d => myRobot.Storage.All(s => myRobot.CanComplete(d)));
                        if (dat == null)
                        {
                            dat = diagnosed.OrderBy(d => myRobot.Storage.Sum(c => c.Value - (d.Cost[c.Key] - myRobot.Expertise[c.Key])))
                                           .ThenBy(d => d.Health).FirstOrDefault();
                            if (dat == null)
                            {
                                dat = diagnosed.First();
                            }
                        }
                        myRobot.Connect(dat.Id.ToString());
                    }
                    else if (myRobot.SampleData.Any(d => !d.Diagnosed || !d.IsPossible()) /* || !canHoldMore*/)
                    {
                        message = "cant finish,";
                        var sam = myRobot.SampleData.FirstOrDefault(d => !d.Diagnosed || !d.IsPossible());
                        if (sam == null)
                        {
                            //throw away the one i'm furthest from completing, with the lowest point value
                            sam = myRobot.SampleData.OrderByDescending(d => d.Paid.Sum(c => c.Value - (d.Cost[c.Key] - myRobot.Expertise[c.Key]))).ThenBy(d => d.Health).FirstOrDefault();
                        }
                        message += sam.Diagnosed ? "get rid" : "diag";
                        myRobot.Connect(sam.Id.ToString());
                    }
                    else if (myRobot.SampleData.Any(d => myRobot.CanComplete(d) || (d.Diagnosed && d.IsPossible() && canHoldMore)))
                    {
                        message = "can complete";
                        myRobot.MoveTo(molecules);
                    }
                    else
                    {
                        message = "Notta";
                        myRobot.MoveTo(samples);
                    }
                }
                else if (myRobot.Target is Molecules)
                {
                    var data = myRobot.SampleData.Where(s => s.IsPossible() && !myRobot.CanComplete(s))
                                                 .OrderBy(d => myRobot.Storage.Sum(c => d.Cost[c.Key] - c.Value - myRobot.Expertise[c.Key]))
                        //.ThenBy(d => d.Cost.Sum(c => c.Value - myRobot.Expertise[c.Key]))
                                                 .FirstOrDefault();
                    if (myRobot.SampleData.Any(s => myRobot.CanComplete(s)))
                    {
                        message = "Have enough";
                        myRobot.MoveTo(laboratory);
                    }
                    else if (data == null || myRobot.Storage.Sum(d => d.Value) >= MaxMolecules)
                    {
                        Console.Error.WriteLine("Data: " + data);

                        if (data == null)
                        {
                            if (diagnosis.CloudData.Count > 0)
                            {
                                message = "Not paid, grabbing from cloud";
                                myRobot.MoveTo(diagnosis);
                            }
                            else
                            {
                                message = "not enough, get sample";
                                // There are not enough molecules, get another sample
                                myRobot.MoveTo(samples);
                            }
                        }
                        else
                        {
                            message = "Storage full";
                            myRobot.MoveTo(diagnosis);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Data: " + data.Id + " " + string.Join(",", data));
                        Console.Error.WriteLine(string.Join(";", data.Cost.Select(d => string.Format("V: {0}, P: {1}, M: {2}", d, data.Paid[d.Key], moleculesAvailable[d.Key] > 0))));
                        var molecule = data.Cost.FirstOrDefault(d => d.Value - myRobot.Expertise[d.Key] > myRobot.Storage[d.Key] && moleculesAvailable[d.Key] > 0);
                        if (molecule.Equals(default(KeyValuePair<MoleculeType, int>)))
                        {
                            myRobot.MoveTo(laboratory);
                        }
                        message = "Grabbing " + molecule.Key;
                        myRobot.Connect(Enum.GetName(typeof(MoleculeType), molecule.Key));
                        data.Paid[molecule.Key]++;
                    }
                }
                else if (myRobot.Target is Laboratory)
                {
                    var ready = myRobot.SampleData.FirstOrDefault(s => myRobot.CanComplete(s));
                    //Not enough molecules to complete
                    if (ready == null)
                    {
                        message = "Not enough!";
                        if (myRobot.SampleData.Any(s => s.IsPossible()))
                        {
                            message = "there's some at the mole station";
                            myRobot.MoveTo(molecules);
                        }
                        else
                        {
                            message = "get another sample";
                            myRobot.MoveTo(samples);
                        }
                    }
                    else // Complete
                    {
                        message = "Fin";
                        myRobot.Connect(ready.Id.ToString());
                        myRobot.SampleData.Clear();
                    }
                }
                else
                {
                    myRobot.MoveTo(samples);
                }
                myRobot.PrintAction(myRobot.TargetName);
            }
        }
    }
}