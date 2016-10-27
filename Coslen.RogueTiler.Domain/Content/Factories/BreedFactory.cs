using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Content.Factories
{

    public class BreedFactory
    {
        private static BreedFactory instance;
        public static BreedFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BreedFactory();
                }
                return instance;
            }
        }
        private BreedFactory()
        {
            initialize();
        }


        /// The default tracking for a breed that doesn't specify it.
        private int _tracking;

        /// The default speed for breeds in the current group. If the breed
        /// specifies a speed, it is added to this.
        private int _speed;

        /// The default meander for breeds in the current group. If the breed
        /// specifies a meander, it is added to this.
        private int _meander;

        /// Default flags for the current group.
        private String _flags;

        /// The current glyph. Any items defined will use this. Can be a string or
        /// a character code.
        private string _glyph;

        public Dictionary<string, Breed> Breeds { get; set; } = new Dictionary<string, Breed>();

        // colour constants
        private const string aqua = "aqua";
        private const string darkAqua = "darkAqua";
        private const string brown = "brown";
        private const string darkGold = "darkGold";
        private const string gray = "gray";
        private const string lightGold = "lightGold";
        private const string gold = "gold";
        private const string lightGray = "lightGray";
        private const string lightAqua = "lightAqua";
        private const string lightBrown = "lightBrown";
        private const string purple = "purple";
        private const string orange = "orange";
        private const string red = "red";
        private const string darkGray = "darkGray";
        private const string lightRed = "lightRed";
        private const string darkRed = "darkRed";
        private const string darkPurple = "darkPurple";
        private const string green = "green";
        private const string lightGreen = "lightGreen";
        private const string darkGreen = "darkGreen";
        private const string lightBlue = "lightBlue";
        private const string lightYellow = "lightYellow";
        private const string lightPurple = "lightPurple";
        private const string blue = "blue";
        private const string lightOrange = "lightOrange"; // = "";
        private const string darkBlue = "darkBlue";
        private const string white = "white";
        private const string yellow = "yellow";
        private const string darkYellow = "darkYellow";
        private const string black = "black";
        private const string darkBrown = "darkBrown";

        private void initialize()
        {
            // a  Arachnid/Scorpion   A  Ancient being
            // b  Giant Bat           B  Bird
            // c  Canine (Dog)        C  Canid (Dog-like humanoid)
            // d  Dragon              D  Ancient Dragon
            // e  Floating Eye        E  Elemental
            // f  Flying Insect       F  Feline (Cat)
            // g  Goblin              G  Golem
            // h  Humanoids           H  Hybrid
            // i  Insect              I  Insubstantial (ghost)
            // j  Jelly/Slime         J  (unused)
            // k  Kobold/Imp/ete   K  Kraken/Land Octopus
            // l  Lizard man          L  Lich
            // m  Mold/Mushroom       M  Multi-Headed Hydra
            // n  Naga                N  Demon
            // o  Orc                 O  Ogre
            // p  Human "person"      P  Giant "person"
            // q  Quadruped           Q  End boss ("quest")
            // r  Rodent/Rabbit       R  Reptile/Amphibian
            // s  Slug                S  Snake
            // t  Troglodyte          T  Troll
            // u  Minor Undead        U  Major Undead
            // v  Vine/Plant          V  Vampire
            // w  Worm or Worm Mass   W  Wight/Wraith
            // x  Skeleton            X  Xorn/Xaren
            // y  Yeek                Y  Yeti
            // z  Zombie/Mummy        Z  Serpent (snake-like dragon)
            // TODO:
            // - Come up with something better than yeeks for "y".
            // - Don't use both "u" and "U" for undead?

            arachnids();
            ancients();
            bats();
            birds();
            canines();
            canids();
            eyes();
            elementals();
            flyingInsects();
            felines();
            goblins();
            golems();
            humanoids();
            hybrids();
            insects();
            insubstantials();
            jellies();         // J unused
            kobolds();
            krakens();
            lizardMen();
            lichs();
            mushrooms();
            hydras();
            nagas();
            demons();
            orcs();
            ogres();
            people();
            giants();
            quadrupeds();
            rodents();
            reptiles();
            slugs();
            snakes();
            worms();
            skeletons();
        }


        private void arachnids()
        {
            group("a", flags: "fearless");
            breed("garden spider", darkAqua, 2,
            new List<object>() {
                new Attack("bite[s]", 2)
            }, drop: DropFactory.PercentDrop(3, "Stinger"),
                meander: 8, flags: "group");

            breed("brown spider", brown, 1, new List<object>() {
              new Attack("bite[s]", 15, ElementFactory.Instance.Poison)
            }, drop: DropFactory.PercentDrop(5, "Stinger"),
                meander: 8);

            breed("giant spider", darkBlue, 20, new List<object>() {
              new Attack("bite[s]", 5, ElementFactory.Instance.Poison)
            }, drop: DropFactory.PercentDrop(10, "Stinger"),
                meander: 5);
        }

        private void ancients()
        {

        }

        private void bats()
        {
            group("b");
            breed("brown bat", lightBrown, 9, new List<object>() {
              new Attack("bite[s]", 4),
            }, meander: 6, speed: 2);

            breed("giant bat", lightBrown, 16, new List<object>() {
              new Attack("bite[s]", 6),


            }, meander: 4, speed: 2);

            breed("cave bat", gray, 10, new List<object>() {
              new Attack("bite[s]", 6),


            }, meander: 3, speed: 3, flags: "group");
        }

        private void birds()
        {
            group("B");
            breed("robin", lightRed, 3, new List<object>() {
              new Attack("claw[s]", 1),


            }, drop: DropFactory.PercentDrop(25, "Red Feather"),
                meander: 4, speed: 2);

            breed("crow", darkGray, 9, new List<object>() {
              new Attack("bite[s]", 5),


            }, drop: DropFactory.PercentDrop(25, "Black Feather"),
                meander: 4, speed: 2, flags: "group");

            breed("raven", gray, 12, new List<object>() {
              new Attack("bite[s]", 5),
              new Attack("claw[s]", 4),


            }, drop: DropFactory.PercentDrop(20, "Black Feather"),
                meander: 1, flags: "protective");
        }

        private void canines()
        {
            group("c", tracking: 20, meander: 3, flags: "few");
            breed("mangy cur", yellow, 11, new List<object>() {
              new Attack("bite[s]", 4),
              MoveFactory.howl(range: 6)
            }, drop: DropFactory.PercentDrop(20, "Fur Pelt"));

            breed("wild dog", gray, 16, new List<object>() {
              new Attack("bite[s]", 4),
              MoveFactory.howl(range: 8)
            }, drop: DropFactory.PercentDrop(20, "Fur Pelt"));

            breed("mongrel", orange, 28, new List<object>() {
              new Attack("bite[s]", 6),
              MoveFactory.howl(range: 10)
            }, drop: DropFactory.PercentDrop(20, "Fur Pelt"));
        }

        private void canids()
        {
        }

        private void eyes()
        {
            group("e", flags: "immobile");
            breed("lazy eye", white, 16, new List<object>() {
              new Attack("gaze[s] into", 6),
              MoveFactory.sparkBolt(rate: 7, damage: 10, range: 5),
              MoveFactory.teleport(rate: 9, range: 4)
            });

            group("e", flags: "immobile");
            breed("floating eye", yellow, 30, new List<object>() {
              new Attack("touch[es]", 4),
              MoveFactory.lightBolt(rate: 5, damage: 16),
              MoveFactory.teleport(rate: 8, range: 7)
            });

            // baleful eye, malevolent eye, murderous eye
        }

        private void elementals()
        {

        }

        private void flyingInsects()
        {
            group("f", tracking: 5, meander: 8);
            breed("butterfl[y|ies]", lightPurple, 1, new List<object>() {
              new Attack("tickle[s] on", 1),
            }, drop: DropFactory.PercentDrop(20, "Insect Wing"),
                speed: 2, flags: "few fearless");

            breed("bee", yellow, 1, new List<object>() {
              new Attack("sting[s]", 2)
            }, speed: 1, flags: "group protective");

            breed("wasp", brown, 1, new List<object>() {
              new Attack("sting[s]", 2, ElementFactory.Instance.Poison),
            }, drop: DropFactory.PercentDrop(30, "Stinger"),
                speed: 2, flags: "berzerk");
        }

        private void felines()
        {
            group("F");
            breed("stray cat", gold, 9, new List<object>() {
              new Attack("bite[s]", 3),
              new Attack("scratch[es]", 2),


            }, drop: DropFactory.PercentDrop(10, "Fur Pelt"),
                meander: 3, speed: 1);
        }

        private void goblins()
        {
            group("g", meander: 1, flags: "open-doors");
            breed("goblin peon", lightBrown, 20, new List<object>() {
              new Attack("stab[s]", 5)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(10, "spear", 3),
              DropFactory.PercentDrop(5, "healing", 2),
            }, meander: 2, flags: "few");

            breed("goblin archer", green, 22, new List<object>() {
                      new Attack("stab[s]", 3),
                      MoveFactory.arrow(rate: 3, damage: 4)
                    }, drop: new List<Drop>() {
           DropFactory.PercentDrop(20, "bow", 1),
           DropFactory.PercentDrop(10, "dagger", 2),
           DropFactory.PercentDrop(5, "healing", 3),


         }, flags: "few");

            breed("goblin fighter", brown, 30, new List<object>() {
                          new Attack("stab[s]", 7)
                        }, drop: new List<Drop>() {
            DropFactory.PercentDrop(15, "spear", 5),
            DropFactory.PercentDrop(10, "armor", 5),
            DropFactory.PercentDrop(5, "resistance", 3),
            DropFactory.PercentDrop(5, "healing", 3),


                        });

            breed("goblin warrior", gray, 42, new List<object>() {
                          new Attack("stab[s]", 10)
                        }, drop: new List<Drop>() {
                          DropFactory.PercentDrop(20, "axe", 6),
                          DropFactory.PercentDrop(20, "armor", 6),
                          DropFactory.PercentDrop(5, "resistance", 3),
                          DropFactory.PercentDrop(5, "healing", 3),
                        }, flags: "protective");

            breed("goblin mage", blue, 30, new List<object>() {
                          new Attack("whip[s]", 7),
                          MoveFactory.fireBolt(rate: 12, damage: 6),
                          MoveFactory.sparkBolt(rate: 12, damage: 8),


                        }, drop: new List<Drop>() {
                          DropFactory.PercentDrop(10, "equipment", 5),
                          DropFactory.PercentDrop(10, "whip", 5),
                          DropFactory.PercentDrop(20, "magic", 6),
                        });

            breed("goblin ranger", darkGreen, 36, new List<object>() {
                          new Attack("stab[s]", 10),
                          MoveFactory.arrow(rate: 3, damage: 8)
                        }, drop: new List<Drop>() {
                          DropFactory.PercentDrop(30, "bow", 11),
                          DropFactory.PercentDrop(20, "armor", 8),
                          DropFactory.PercentDrop(20, "magic", 8)
                        });

            // TODO: Always drop something good.
            breed("Erlkonig, the Goblin Prince", darkGray, 80, new List<object>() {
                          new Attack("hit[s]", 10),
                          new Attack("slash[es]", 14),
                          MoveFactory.darkBolt(rate: 20, damage: 10),


                        }, drop: new List<Drop>() {
                            DropFactory.PercentDrop(60, "equipment", 10),
              DropFactory.PercentDrop(60, "equipment", 10),
              DropFactory.PercentDrop(40, "magic", 12),

            }, flags: "protective");
        }

        private void golems()
        {
        }

        private void humanoids()
        {
        }

        private void hybrids()
        {
        }

        private void insects()
        {
            group("i", tracking: 3, meander: 8, flags: "fearless");
            breed("giant cockroach[es]", darkBrown, 4, new List<object>() {
              new Attack("crawl[s] on", 3),
              MoveFactory.spawn(rate: 4)
            }, drop: DropFactory.PercentDrop(10, "Insect Wing"),
                speed: 3);

            breed("giant centipede", red, 16, new List<object>() {
              new Attack("crawl[s] on", 3),
              new Attack("bite[s]", 5),


            }, speed: 3, meander: 0);
        }

        private void insubstantials()
        {

        }

        private void jellies()
        {
            group("j", meander: 4, speed: -1, tracking: 4, flags: "few fearless");
            breed("green slime", green, 12, new List<object>() {
              new Attack("crawl[s] on", 4),
              MoveFactory.spawn(rate: 6)
            });

            breed("frosty slime", white, 14, new List<object>() {
              new Attack("crawl[s] on", 5, ElementFactory.Instance.Cold),
              MoveFactory.spawn(rate: 6)
            });

            breed("smoking slime", red, 18, new List<object>() {
              new Attack("crawl[s] on", 6, ElementFactory.Instance.Fire),
              MoveFactory.spawn(rate: 6)
            });

            breed("sparkling slime", lightPurple, 22, new List<object>() {
              new Attack("crawl[s] on", 8, ElementFactory.Instance.Lightning),
              MoveFactory.spawn(rate: 6)
            });
        }

        private void kobolds()
        {
            group("k", speed: 2, meander: 4, flags: "cowardly");
            breed("forest sprite", lightGreen, 8, new List<object>() {
              new Attack("scratch[es]", 4),
              MoveFactory.teleport(range: 6)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(20, "magic", 1)
            });

            breed("house sprite", lightBlue, 15, new List<object>() {
              new Attack("poke[s]", 8),
              MoveFactory.teleport(range: 6)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(20, "magic", 6)
            });

            breed("mischievous sprite", lightRed, 24, new List<object>() {
              new Attack("stab[s]", 9),
              MoveFactory.sparkBolt(rate: 8, damage: 8),
              MoveFactory.poisonBolt(rate: 15, damage: 10),
              MoveFactory.teleport(range: 8)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(40, "magic", 8)
            });

            breed("scurrilous imp", lightRed, 18, new List<object>() {
              new Attack("club[s]", 4),
              MoveFactory.insult(),
              MoveFactory.haste()
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(10, "club", 1),
              DropFactory.PercentDrop(5, "speed", 1),


            }, meander: 4, flags: "cowardly");

            breed("vexing imp", purple, 19, new List<object>() {
              new Attack("scratch[es]", 4),
              MoveFactory.insult(),
              MoveFactory.sparkBolt(rate: 5, damage: 6)
            }, drop: DropFactory.PercentDrop(10, "teleportation", 1),
                meander: 2, speed: 1, flags: "cowardly");

            breed("kobold", red, 16, new List<object>() {
              new Attack("poke[s]", 4),
              MoveFactory.teleport(rate: 6, range: 6)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(30, "magic", 7)
            }, meander: 2, flags: "group");

            breed("kobold shaman", blue, 16, new List<object>() {
              new Attack("hit[s]", 4),
              MoveFactory.teleport(rate: 5, range: 6),
              MoveFactory.waterBolt(rate: 5, damage: 6)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(40, "magic", 7)
            }, meander: 2);

            breed("kobold trickster", gold, 20, new List<object>() {
              new Attack("hit[s]", 5),
              MoveFactory.sparkBolt(rate: 5, damage: 8),
              MoveFactory.teleport(rate: 5, range: 6),
              MoveFactory.haste(rate: 7)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(40, "magic", 7)
            }, meander: 2);

            breed("kobold priest", white, 25, new List<object>() {
              new Attack("club[s]", 6),
              MoveFactory.heal(rate: 15, amount: 10),
              MoveFactory.fireBolt(rate: 10, damage: 8),
              MoveFactory.teleport(rate: 5, range: 6),
              MoveFactory.haste(rate: 7)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(30, "club", 10),
              DropFactory.PercentDrop(40, "magic", 7)
            }, meander: 2);

            breed("imp incanter", lightPurple, 18, new List<object>() {
              new Attack("scratch[es]", 4),
              MoveFactory.insult(),
              MoveFactory.fireBolt(rate: 5, damage: 10)
            }, drop: DropFactory.PercentDrop(20, "magic", 1),
                meander: 4, speed: 1, flags: "cowardly");

            breed("imp warlock", darkPurple, 40, new List<object>() {
              new Attack("stab[s]", 5),
              MoveFactory.iceBolt(rate: 8, damage: 12),
              MoveFactory.fireBolt(rate: 8, damage: 12)
            }, drop: DropFactory.PercentDrop(20, "magic", 4),
                meander: 3, speed: 1, flags: "cowardly");

            // TODO: Always drop something good.
            breed("Feng", orange, 60, new List<object>() {
              new Attack("stab[s]", 5),
              MoveFactory.teleport(rate: 5, range: 6),
              MoveFactory.teleport(rate: 50, range: 30),
              MoveFactory.insult(),
              MoveFactory.lightningCone(rate: 8, damage: 12)
            }, drop: DropFactory.PercentDrop(20, "magic", 4),
                meander: 3, speed: 1, flags: "cowardly");

            // homonculous
        }

        private void krakens()
        {

        }

        private void lizardMen()
        {
            // troglodyte
            // reptilian
        }

        private void lichs()
        {

        }

        private void mushrooms()
        {

        }

        private void hydras()
        {

        }

        private void nagas()
        {

        }

        private void demons()
        {

        }

        private void orcs()
        {

        }

        private void ogres()
        {

        }

        private void people()
        {
            group("p", tracking: 14, flags: "open-doors");
            breed("simpering knave", orange, 15, new List<object>() {
              new Attack("hit[s]", 2),
              new Attack("stab[s]", 4)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(30, "whip", 1),
              DropFactory.PercentDrop(20, "body", 1),
              DropFactory.PercentDrop(10, "boots", 2),
              DropFactory.PercentDrop(10, "magic", 1),


            }, meander: 3, flags: "cowardly");

            breed("decrepit mage", purple, 16, new List<object>() {
              new Attack("hit[s]", 2),
              MoveFactory.sparkBolt(rate: 10, damage: 8)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(30, "magic", 3),
              DropFactory.PercentDrop(15, "dagger", 1),
              DropFactory.PercentDrop(15, "staff", 1),
              DropFactory.PercentDrop(10, "robe", 2),
              DropFactory.PercentDrop(10, "boots", 2)
            }, meander: 2);

            breed("unlucky ranger", green, 20, new List<object>() {
              new Attack("slash[es]", 2),
              MoveFactory.arrow(rate: 4, damage: 2)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(15, "potion", 3),
              DropFactory.PercentDrop(10, "bow", 4),
              DropFactory.PercentDrop(5, "sword", 4),
              DropFactory.PercentDrop(8, "body", 3)
            }, meander: 2);

            breed("drunken priest", aqua, 18, new List<object>() {
              new Attack("hit[s]", 3),
              MoveFactory.heal(rate: 15, amount: 8)
            }, drop: new List<Drop>() {
              DropFactory.PercentDrop(15, "scroll", 3),
              DropFactory.PercentDrop(7, "club", 2),
              DropFactory.PercentDrop(7, "robe", 2)
            }, meander: 4, flags: "fearless");
        }

        private void giants()
        {

        }

        private void quadrupeds()
        {
            group("q");
            breed("fox", orange, 20, new List<object>() {
              new Attack("bite[s]", 5),
              new Attack("scratch[es]", 4)
            }, drop: "Fox Pelt",
                meander: 1);
        }

        private void rodents()
        {
            group("r", meander: 4);
            breed("field [mouse|mice]", lightBrown, 3, new List<object>() {
              new Attack("bite[s]", 3),
              new Attack("scratch[es]", 2)
            }, speed: 1);

            breed("fuzzy bunn[y|ies]", lightBlue, 14, new List<object>() {
              new Attack("bite[s]", 5),
              new Attack("kick[s]", 4)
            }, meander: -2);

            breed("vole", gray, 5, new List<object>() {
              new Attack("bite[s]", 4)
            }, speed: 1);

            breed("white [mouse|mice]", white, 6, new List<object>() {
              new Attack("bite[s]", 5),
              new Attack("scratch[es]", 3)
            }, speed: 1);

            breed("sewer rat", darkGray, 7, new List<object>() {
              new Attack("bite[s]", 4),
              new Attack("scratch[es]", 3)
            }, meander: -1, speed: 1, flags: "group");

            breed("plague rat", darkGreen, 10, new List<object>() {
              new Attack("bite[s]", 4, ElementFactory.Instance.Poison),
              new Attack("scratch[es]", 3)
            }, speed: 1, flags: "group");
        }

        private void reptiles()
        {
            group("R");
            breed("frog", green, 4, new List<object>() {
              new Attack("hop[s] on", 2),


            }, meander: 4, speed: 1);

            // TODO: Drop scales?
            group("R", meander: 1, flags: "fearless");
            breed("lizard guard", yellow, 26, new List<object>() {
              new Attack("claw[s]", 8),
              new Attack("bite[s]", 10),


            });

            breed("lizard protector", darkYellow, 30, new List<object>() {
              new Attack("claw[s]", 10),
              new Attack("bite[s]", 14),


            });

            breed("armored lizard", gray, 38, new List<object>() {
              new Attack("claw[s]", 10),
              new Attack("bite[s]", 15),


            });

            breed("scaled guardian", darkGray, 50, new List<object>() {
              new Attack("claw[s]", 10),
              new Attack("bite[s]", 15),


            });

            breed("saurian", orange, 64, new List<object>() {
              new Attack("claw[s]", 12),
              new Attack("bite[s]", 17),


            });

            group("R", meander: 3);
            breed("juvenile salamander", lightRed, 24, new List<object>() {
              new Attack("bite[s]", 12, ElementFactory.Instance.Fire),
              MoveFactory.fireCone(rate: 16, damage: 18, range: 6)
            });

            breed("salamander", red, 40, new List<object>() {
              new Attack("bite[s]", 16, ElementFactory.Instance.Fire),
              MoveFactory.fireCone(rate: 16, damage: 24, range: 8)
            });
        }

        private void slugs()
        {
            group("s", tracking: 2, flags: "fearless", meander: 1, speed: -3);
            breed("slug", darkYellow, 6, new List<object>() {
              new Attack("crawl[s] on", 3),


            });

            breed("giant slug", green, 20, new List<object>() {
              new Attack("crawl[s] on", 7, ElementFactory.Instance.Poison),


            });
        }

        private void snakes()
        {
            group("S", meander: 4);
            breed("garter snake", gold, 7, new List<object>() {
              new Attack("bite[s]", 1),


            });

            breed("tree snake", lightGreen, 14, new List<object>() {
              new Attack("bite[s]", 4),


            });

            breed("cave snake", gray, 35, new List<object>() {
              new Attack("bite[s]", 10),


            });
        }

        private void worms()
        {
            group("w", meander: 4, flags: "fearless");
            breed("giant earthworm", lightRed, 20, new List<object>() {
              new Attack("crawl[s] on", 4),


            }, speed: -2);

            breed("blood worm", red, 4, new List<object>() {
              new Attack("crawl[s] on", 5),


            }, flags: "swarm");

            breed("giant cave worm", white, 36, new List<object>() { new Attack("crawl[s] on", 8, ElementFactory.Instance.Acid), }, speed: -2);

            breed("fire worm", orange, 6, new List<object>() {
              new Attack("crawl[s] on", 5, ElementFactory.Instance.Fire),


            }, flags: "swarm");
        }

        private void skeletons()
        {

        }


        #region Utility Functions

        private void group(string glyph, int? meander = null, int? speed = null, int? tracking = null, string flags = null)
        {
            _glyph = glyph;
            _meander = meander.HasValue ? meander.Value : 0;
            _speed = speed.HasValue ? speed.Value : 0;
            _tracking = tracking.HasValue ? tracking.Value : 10;
            _flags = flags;
        }

        private Breed breed(string name, string foreColour, int maxHealth, List<object> actions,
                            object drop = null, int? tracking = null, int meander = 0, int speed = 0, string flags = null // All optional
                       )
        {
            if (tracking == null) tracking = _tracking;

            var attacks = new List<Attack>();
            var moves = new List<Move>();

            foreach (var action in actions)
            {
                if (action is Attack) attacks.Add(action as Attack);
                if (action is Move) moves.Add(action as Move);
            }

            Drop processedDrop = null;
            if (drop is List<Drop>)
            {
                processedDrop = DropFactory.dropAllOf(drop as List<Drop>);
            }
            else if (drop is Drop)
            {
                processedDrop = DropFactory.dropAllOf(new List<Drop>() { drop as Drop });
            }
            else if (drop is String)
            {
                processedDrop = DropFactory.parseDrop(drop as string);
            }
            else
            {
                // Non-null way of dropping nothing.
                processedDrop = DropFactory.dropAllOf(new List<Drop>());
            }

            var flagSet = new List<string>();
            if (_flags != null) flagSet.AddRange(_flags.Split(' '));
            if (flags != null) flagSet.AddRange(flags.Split(' '));

            var breed = new Breed(
                    name, Pronouns.It, new Glyph(_glyph, foreColour),
                    attacks, moves,
                    processedDrop,
                    maxHealth: maxHealth,
                    tracking: tracking.Value,
                    meander: meander + _meander,
                    speed: speed + _speed,
                    flags: flagSet,
                    slug: string.Empty);

            Breeds.Add(breed.Name, breed);
            return breed;
        }

        #endregion
    }
}
