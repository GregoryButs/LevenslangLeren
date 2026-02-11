namespace MusicApp.Models
{

    public class AlbumRepository
    {
        private List<Album> _albums;

        public AlbumRepository()
        {
            if (_albums == null)
            {
                InitializeData();
            }
        }

        public List<Album> GetAll()
        {
            return new List<Album>(_albums);
        }

        public Album FindById(int id)
        {
            return _albums.Find(x => x.Id == id);
        }

        private void InitializeData()
        {
            _albums = new List<Album>
            {
              new Album
              {
                Id = 1,
                Title = "×",
                Artist = "Ed Sheeran",
                ImageURL = "http://coverartarchive.org/release/c34cf176-46b5-4533-8287-a9f944599e69/7597444557-500.jpg",
                Rating = 3.5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "One",
                    Length = 252
                  },
                  new Song
                  {
                    Title = "I’m a Mess",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "Sing",
                    Length = 235
                  },
                  new Song
                  {
                    Title = "Don’t",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "Nina",
                    Length = 225
                  },
                  new Song
                  {
                    Title = "Photograph",
                    Length = 258
                  },
                  new Song
                  {
                    Title = "Bloodstream",
                    Length = 300
                  },
                  new Song
                  {
                    Title = "Tenerife Sea",
                    Length = 241
                  },
                  new Song
                  {
                    Title = "Runaway",
                    Length = 205
                  },
                  new Song
                  {
                    Title = "The Man",
                    Length = 250
                  },
                  new Song
                  {
                    Title = "Thinking Out Loud",
                    Length = 281
                  },
                  new Song
                  {
                    Title = "Afire Love",
                    Length = 314
                  },
                  new Song
                  {
                    Title = "Take It Back",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "Shirtsleeves",
                    Length = 190
                  },
                  new Song
                  {
                    Title = "Even My Dad Does Sometimes",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "I See Fire",
                    Length = 299
                  },
                  new Song
                  {
                    Title = "All of the Stars",
                    Length = 237
                  }
                }
              },
              new Album
              {
                Id = 2,
                Title = "1989",
                Artist = "Taylor Swift",
                ImageURL = "http://coverartarchive.org/release/907d30af-234e-4b63-b6a7-34ee88929e2b/29583942554-500.jpg",
                Rating = 4.2d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Welcome to New York",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "Blank Space",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Style",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Out of the Woods",
                    Length = 235
                  },
                  new Song
                  {
                    Title = "All You Had to Do Was Stay",
                    Length = 193
                  },
                  new Song
                  {
                    Title = "Shake It Off",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "I Wish You Would",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Bad Blood",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Wildest Dreams",
                    Length = 220
                  },
                  new Song
                  {
                    Title = "How You Get the Girl",
                    Length = 247
                  },
                  new Song
                  {
                    Title = "This Love",
                    Length = 250
                  },
                  new Song
                  {
                    Title = "I Know Places",
                    Length = 195
                  },
                  new Song
                  {
                    Title = "Clean",
                    Length = 271
                  }
                }
              },
              new Album
              {
                Id = 3,
                Title = "A Few Small Repairs",
                Artist = "Shawn Colvin",
                ImageURL = "http://coverartarchive.org/release/64e8c9e5-3f0a-4b85-9a8b-1c4521563c5d/21026184931-500.jpg",
                Rating = 5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Sunny Came Home",
                    Length = 265
                  },
                  new Song
                  {
                    Title = "Get Out of This House",
                    Length = 254
                  },
                  new Song
                  {
                    Title = "The Facts About Jimmy",
                    Length = 323
                  },
                  new Song
                  {
                    Title = "You and the Mona Lisa",
                    Length = 245
                  },
                  new Song
                  {
                    Title = "Trouble",
                    Length = 258
                  },
                  new Song
                  {
                    Title = "I Want It Back",
                    Length = 297
                  },
                  new Song
                  {
                    Title = "If I Were Brave",
                    Length = 192
                  },
                  new Song
                  {
                    Title = "Wichita Skyline",
                    Length = 218
                  },
                  new Song
                  {
                    Title = "84,000 Different Delusions",
                    Length = 242
                  },
                  new Song
                  {
                    Title = "Suicide Alley",
                    Length = 330
                  },
                  new Song
                  {
                    Title = "New Thing Now",
                    Length = 220
                  },
                  new Song
                  {
                    Title = "Nothin’ on Me",
                    Length = 236
                  }
                }
              },
              new Album
              {
                Id = 4,
                Title = "All for You",
                Artist = "Janet Jackson",
                ImageURL = "http://coverartarchive.org/release/2f0b6333-f410-4cbc-9093-c6e7ca578de8/9562862080-500.jpg",
                Rating = 3.45d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Intro",
                    Length = 60
                  },
                  new Song
                  {
                    Title = "You Ain’t Right",
                    Length = 272
                  },
                  new Song
                  {
                    Title = "All for You",
                    Length = 329
                  },
                  new Song
                  {
                    Title = "2wayforyou (interlude)",
                    Length = 19
                  },
                  new Song
                  {
                    Title = "Come On Get Up",
                    Length = 287
                  },
                  new Song
                  {
                    Title = "When We Oooo",
                    Length = 274
                  },
                  new Song
                  {
                    Title = "China Love",
                    Length = 276
                  },
                  new Song
                  {
                    Title = "Love Scene (Ooh Baby)",
                    Length = 256
                  },
                  new Song
                  {
                    Title = "Would You Mind",
                    Length = 332
                  },
                  new Song
                  {
                    Title = "Lame (interlude)",
                    Length = 11
                  },
                  new Song
                  {
                    Title = "Trust a Try",
                    Length = 316
                  },
                  new Song
                  {
                    Title = "Clouds (interlude)",
                    Length = 19
                  },
                  new Song
                  {
                    Title = "Son of a Gun (I Betcha Think This Song Is About You)",
                    Length = 356
                  },
                  new Song
                  {
                    Title = "Truth",
                    Length = 405
                  },
                  new Song
                  {
                    Title = "Theory (interlude)",
                    Length = 26
                  },
                  new Song
                  {
                    Title = "Someone to Call My Lover",
                    Length = 272
                  },
                  new Song
                  {
                    Title = "Feels So Right",
                    Length = 282
                  },
                  new Song
                  {
                    Title = "Doesn’t Really Matter",
                    Length = 264
                  },
                  new Song
                  {
                    Title = "Better Days",
                    Length = 305
                  },
                  new Song
                  {
                    Title = "Outro",
                    Length = 9
                  }
                }
              },
              new Album
              {
                Id = 5,
                Title = "Back to Basics",
                Artist = "Christina Aguilera",
                ImageURL = "http://coverartarchive.org/release/e35e0144-76a3-4a7b-a200-7987ca4d3ff4/14959159087-500.jpg",
                Rating = 3.65d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Intro (Back to Basics)",
                    Length = 107
                  },
                  new Song
                  {
                    Title = "Makes Me Wanna Pray",
                    Length = 250
                  },
                  new Song
                  {
                    Title = "Back in the Day",
                    Length = 253
                  },
                  new Song
                  {
                    Title = "Ain’t No Other Man",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "Understand",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "Slow Down Baby",
                    Length = 209
                  },
                  new Song
                  {
                    Title = "Oh Mother",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "F.U.S.S.",
                    Length = 141
                  },
                  new Song
                  {
                    Title = "On Our Way",
                    Length = 216
                  },
                  new Song
                  {
                    Title = "Without You",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "Still Dirrty",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "Here to Stay",
                    Length = 199
                  },
                  new Song
                  {
                    Title = "Thank You (Dedication to Fans…)",
                    Length = 298
                  }
                }
              },
              new Album
              {
                Id = 6,
                Title = "Back to Black",
                Artist = "Amy Winehouse",
                ImageURL = "http://coverartarchive.org/release/ad0833f5-b669-4e8b-b736-1fad3eaa3ac8/31222398643-500.jpg",
                Rating = 4.15d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Rehab (album version)",
                    Length = 214
                  },
                  new Song
                  {
                    Title = "You Know I’m No Good",
                    Length = 257
                  },
                  new Song
                  {
                    Title = "Me & Mr Jones",
                    Length = 153
                  },
                  new Song
                  {
                    Title = "Just Friends",
                    Length = 193
                  },
                  new Song
                  {
                    Title = "Back to Black",
                    Length = 241
                  },
                  new Song
                  {
                    Title = "Love Is a Losing Game",
                    Length = 155
                  },
                  new Song
                  {
                    Title = "Tears Dry on Their Own",
                    Length = 186
                  },
                  new Song
                  {
                    Title = "Wake Up Alone",
                    Length = 222
                  },
                  new Song
                  {
                    Title = "Some Unholy War",
                    Length = 142
                  },
                  new Song
                  {
                    Title = "He Can Only Hold Her",
                    Length = 166
                  },
                  new Song
                  {
                    Title = "You Know I’m No Good (remix)",
                    Length = 202
                  }
                }
              },
              new Album
              {
                Id = 7,
                Title = "Bangerz",
                Artist = "Miley Cyrus",
                ImageURL = "http://coverartarchive.org/release/37e52b37-8efb-4e50-9e10-4508abe239f0/5130280452-500.jpg",
                Rating = 2.5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Adore You",
                    Length = 278
                  },
                  new Song
                  {
                    Title = "We Can’t Stop",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "SMS (Bangerz)",
                    Length = 169
                  },
                  new Song
                  {
                    Title = "4x4",
                    Length = 191
                  },
                  new Song
                  {
                    Title = "My Darlin’",
                    Length = 243
                  },
                  new Song
                  {
                    Title = "Wrecking Ball",
                    Length = 222
                  },
                  new Song
                  {
                    Title = "Love Money Party",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "#GETITRIGHT",
                    Length = 264
                  },
                  new Song
                  {
                    Title = "Drive",
                    Length = 255
                  },
                  new Song
                  {
                    Title = "FU",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Do My Thang",
                    Length = 225
                  },
                  new Song
                  {
                    Title = "Maybe You’re Right",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "Someone Else",
                    Length = 288
                  }
                }
              },
              new Album
              {
                Id = 8,
                Title = "Battle Studies",
                Artist = "John Mayer",
                ImageURL = "http://coverartarchive.org/release/35cb7935-5c9f-4d2d-b9e9-cd63c32453c4/3717881334-500.jpg",
                Rating = 3.65d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Heartbreak Warfare",
                    Length = 269
                  },
                  new Song
                  {
                    Title = "All We Ever Do Is Say Goodbye",
                    Length = 275
                  },
                  new Song
                  {
                    Title = "Half of My Heart",
                    Length = 250
                  },
                  new Song
                  {
                    Title = "Who Says",
                    Length = 176
                  },
                  new Song
                  {
                    Title = "Perfectly Lonely",
                    Length = 268
                  },
                  new Song
                  {
                    Title = "Assassin",
                    Length = 314
                  },
                  new Song
                  {
                    Title = "Crossroads",
                    Length = 149
                  },
                  new Song
                  {
                    Title = "War of My Life",
                    Length = 255
                  },
                  new Song
                  {
                    Title = "Edge of Desire",
                    Length = 332
                  },
                  new Song
                  {
                    Title = "Do You Know Me",
                    Length = 150
                  },
                  new Song
                  {
                    Title = "Friends, Lovers or Nothing",
                    Length = 359
                  }
                }
              },
              new Album
              {
                Id = 9,
                Title = "Beautiful Trauma",
                Artist = "P!nk",
                ImageURL = "http://coverartarchive.org/release/f4d59d38-fba2-44cb-a699-9aa351674f7a/17484531303-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Beautiful Trauma",
                    Length = 250
                  },
                  new Song
                  {
                    Title = "Revenge",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "Whatever You Want",
                    Length = 242
                  },
                  new Song
                  {
                    Title = "What About Us",
                    Length = 269
                  },
                  new Song
                  {
                    Title = "But We Lost It",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Barbies",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "Where We Go",
                    Length = 267
                  },
                  new Song
                  {
                    Title = "For Now",
                    Length = 216
                  },
                  new Song
                  {
                    Title = "Secrets",
                    Length = 210
                  },
                  new Song
                  {
                    Title = "Better Life",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "I Am Here",
                    Length = 246
                  },
                  new Song
                  {
                    Title = "Wild Hearts Can’t Be Broken",
                    Length = 201
                  },
                  new Song
                  {
                    Title = "You Get My Love",
                    Length = 311
                  }
                }
              },
              new Album
              {
                Id = 10,
                Title = "Believe",
                Artist = "Cher",
                ImageURL = "http://coverartarchive.org/release/df4eadee-25f8-3fbc-8da5-ec15804313de/22041737634-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Believe",
                    Length = 238
                  },
                  new Song
                  {
                    Title = "The Power",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "Runaway",
                    Length = 286
                  },
                  new Song
                  {
                    Title = "All or Nothing",
                    Length = 238
                  },
                  new Song
                  {
                    Title = "Strong Enough (single version)",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "Dov’è l’amore",
                    Length = 258
                  },
                  new Song
                  {
                    Title = "Takin’ Back My Heart",
                    Length = 272
                  },
                  new Song
                  {
                    Title = "Taxi Taxi",
                    Length = 304
                  },
                  new Song
                  {
                    Title = "Love Is the Groove",
                    Length = 271
                  },
                  new Song
                  {
                    Title = "We All Sleep Alone",
                    Length = 310
                  }
                }
              },
              new Album
              {
                Id = 11,
                Title = "Born This Way",
                Artist = "Lady Gaga",
                ImageURL = "http://coverartarchive.org/release/dab4bacd-e40e-4d27-80f3-2837b21c7f97/31843171727-500.jpg",
                Rating = 4.05d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Marry the Night",
                    Length = 264
                  },
                  new Song
                  {
                    Title = "Born This Way",
                    Length = 260
                  },
                  new Song
                  {
                    Title = "Government Hooker",
                    Length = 254
                  },
                  new Song
                  {
                    Title = "Judas",
                    Length = 249
                  },
                  new Song
                  {
                    Title = "Americano",
                    Length = 246
                  },
                  new Song
                  {
                    Title = "Hair",
                    Length = 308
                  },
                  new Song
                  {
                    Title = "Scheiße",
                    Length = 225
                  },
                  new Song
                  {
                    Title = "Bloody Mary",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "Bad Kids",
                    Length = 230
                  },
                  new Song
                  {
                    Title = "Highway Unicorn (Road to Love)",
                    Length = 255
                  },
                  new Song
                  {
                    Title = "Heavy Metal Lover",
                    Length = 252
                  },
                  new Song
                  {
                    Title = "Electric Chapel",
                    Length = 252
                  },
                  new Song
                  {
                    Title = "You and I",
                    Length = 307
                  },
                  new Song
                  {
                    Title = "The Edge of Glory",
                    Length = 320
                  }
                }
              },
              new Album
              {
                Id = 12,
                Title = "Brainwashed",
                Artist = "George Harrison",
                ImageURL = "http://coverartarchive.org/release/6ea58be4-1b7e-466d-999c-52e78b93e958/30980705226-500.jpg",
                Rating = 3.5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Any Road",
                    Length = 232
                  },
                  new Song
                  {
                    Title = "P2 Vatican Blues (Last Saturday Night)",
                    Length = 158
                  },
                  new Song
                  {
                    Title = "Pisces Fish",
                    Length = 292
                  },
                  new Song
                  {
                    Title = "Looking for My Life",
                    Length = 229
                  },
                  new Song
                  {
                    Title = "Rising Sun",
                    Length = 327
                  },
                  new Song
                  {
                    Title = "Marwa Blues",
                    Length = 221
                  },
                  new Song
                  {
                    Title = "Stuck Inside a Cloud",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "Run So Far",
                    Length = 245
                  },
                  new Song
                  {
                    Title = "Never Get Over You",
                    Length = 205
                  },
                  new Song
                  {
                    Title = "Between the Devil and the Deep Blue Sea",
                    Length = 154
                  },
                  new Song
                  {
                    Title = "Rocking Chair in Hawaii",
                    Length = 187
                  },
                  new Song
                  {
                    Title = "Brainwashed",
                    Length = 367
                  }
                }
              },
              new Album
              {
                Id = 13,
                Title = "Britney",
                Artist = "Britney Spears",
                ImageURL = "http://coverartarchive.org/release/7ea2150f-7c49-48cd-9d69-b241c010971b/29341703079-500.jpg",
                Rating = 4.25d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "I’m a Slave 4 U",
                    Length = 203
                  },
                  new Song
                  {
                    Title = "Overprotected",
                    Length = 198
                  },
                  new Song
                  {
                    Title = "Lonely",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "I’m Not a Girl, Not Yet a Woman",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Boys",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "Anticipating",
                    Length = 196
                  },
                  new Song
                  {
                    Title = "I Love Rock ’n’ Roll",
                    Length = 186
                  },
                  new Song
                  {
                    Title = "Cinderella",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "Let Me Be",
                    Length = 171
                  },
                  new Song
                  {
                    Title = "Bombastic Love",
                    Length = 185
                  },
                  new Song
                  {
                    Title = "That’s Where You Take Me",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "What It’s Like to Be Me",
                    Length = 170
                  }
                }
              },
              new Album
              {
                Id = 14,
                Title = "Camila",
                Artist = "Camila Cabello",
                ImageURL = "http://coverartarchive.org/release/a44f0bd6-8ba3-4305-b645-12e78109d0fd/18835436506-500.jpg",
                Rating = 4.65d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Never Be the Same",
                    Length = 227
                  },
                  new Song
                  {
                    Title = "All These Years",
                    Length = 164
                  },
                  new Song
                  {
                    Title = "She Loves Control",
                    Length = 177
                  },
                  new Song
                  {
                    Title = "Havana",
                    Length = 216
                  },
                  new Song
                  {
                    Title = "Inside Out",
                    Length = 182
                  },
                  new Song
                  {
                    Title = "Consequences",
                    Length = 178
                  },
                  new Song
                  {
                    Title = "Real Friends",
                    Length = 214
                  },
                  new Song
                  {
                    Title = "Something’s Gotta Give",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "In the Dark",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "Into It",
                    Length = 175
                  },
                  new Song
                  {
                    Title = "Never Be the Same",
                    Length = 227
                  },
                  new Song
                  {
                    Title = "Havana (remix)",
                    Length = 199
                  }
                }
              },
              new Album
              {
                Id = 15,
                Title = "Celebrity",
                Artist = "*NSYNC",
                ImageURL = "http://coverartarchive.org/release/551447f3-286f-44fb-a13b-d9a303aa0950/31674628052-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Pop",
                    Length = 238
                  },
                  new Song
                  {
                    Title = "Celebrity",
                    Length = 198
                  },
                  new Song
                  {
                    Title = "The Game Is Over",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Girlfriend",
                    Length = 254
                  },
                  new Song
                  {
                    Title = "The Two of Us",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Gone",
                    Length = 293
                  },
                  new Song
                  {
                    Title = "Tell Me, Tell Me… Baby",
                    Length = 218
                  },
                  new Song
                  {
                    Title = "Up Against the Wall",
                    Length = 217
                  },
                  new Song
                  {
                    Title = "See Right Through You",
                    Length = 175
                  },
                  new Song
                  {
                    Title = "Selfish",
                    Length = 260
                  },
                  new Song
                  {
                    Title = "Just Don’t Tell Me That",
                    Length = 183
                  },
                  new Song
                  {
                    Title = "Something Like You",
                    Length = 255
                  },
                  new Song
                  {
                    Title = "Do Your Thing",
                    Length = 260
                  }
                }
              },
              new Album
              {
                Id = 16,
                Title = "Changes",
                Artist = "Justin Bieber",
                ImageURL = "http://coverartarchive.org/release/77aebad1-977c-4c92-97d6-17bc46690b06/25423466526-500.jpg",
                Rating = 4.5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "All Around Me",
                    Length = 136
                  },
                  new Song
                  {
                    Title = "Habitual",
                    Length = 168
                  },
                  new Song
                  {
                    Title = "Come Around Me",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "Intentions",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "Yummy",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "Available",
                    Length = 195
                  },
                  new Song
                  {
                    Title = "Forever",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "Running Over",
                    Length = 179
                  },
                  new Song
                  {
                    Title = "Take It Out on Me",
                    Length = 178
                  },
                  new Song
                  {
                    Title = "Second Emotion",
                    Length = 202
                  },
                  new Song
                  {
                    Title = "Get Me",
                    Length = 185
                  },
                  new Song
                  {
                    Title = "E.T.A.",
                    Length = 176
                  },
                  new Song
                  {
                    Title = "Changes",
                    Length = 135
                  },
                  new Song
                  {
                    Title = "Confirmation",
                    Length = 170
                  },
                  new Song
                  {
                    Title = "That’s What Love Is",
                    Length = 165
                  },
                  new Song
                  {
                    Title = "At Least for Now",
                    Length = 149
                  }
                }
              },
              new Album
              {
                Id = 17,
                Title = "Chaos and Creation in the Backyard",
                Artist = "Paul McCartney",
                ImageURL = "http://coverartarchive.org/release/55ccb6a9-c5b2-3a31-9503-829ebb9b4830/19603088026-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Fine Line",
                    Length = 185
                  },
                  new Song
                  {
                    Title = "How Kind of You",
                    Length = 287
                  },
                  new Song
                  {
                    Title = "Jenny Wren",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "At the Mercy",
                    Length = 157
                  },
                  new Song
                  {
                    Title = "Friends to Go",
                    Length = 163
                  },
                  new Song
                  {
                    Title = "English Tea",
                    Length = 132
                  },
                  new Song
                  {
                    Title = "Too Much Rain",
                    Length = 204
                  },
                  new Song
                  {
                    Title = "A Certain Softness",
                    Length = 161
                  },
                  new Song
                  {
                    Title = "Riding to Vanity Fair",
                    Length = 306
                  },
                  new Song
                  {
                    Title = "Follow Me",
                    Length = 151
                  },
                  new Song
                  {
                    Title = "Promise to You Girl",
                    Length = 189
                  },
                  new Song
                  {
                    Title = "This Never Happened Before",
                    Length = 204
                  },
                  new Song
                  {
                    Title = "Anyway / I’ve Only Got Two Hands",
                    Length = 442
                  }
                }
              },
              new Album
              {
                Id = 18,
                Title = "Dangerous Woman",
                Artist = "Ariana Grande",
                ImageURL = "http://coverartarchive.org/release/44f1a2fe-87be-4bbf-ac5b-70fde2be7083/24562959236-500.jpg",
                Rating = 4.85d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Moonlight",
                    Length = 202
                  },
                  new Song
                  {
                    Title = "Dangerous Woman",
                    Length = 235
                  },
                  new Song
                  {
                    Title = "Be Alright",
                    Length = 179
                  },
                  new Song
                  {
                    Title = "Into You",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "Side to Side",
                    Length = 225
                  },
                  new Song
                  {
                    Title = "Let Me Love You",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "Greedy",
                    Length = 214
                  },
                  new Song
                  {
                    Title = "Leave Me Lonely",
                    Length = 229
                  },
                  new Song
                  {
                    Title = "Everyday",
                    Length = 194
                  },
                  new Song
                  {
                    Title = "Sometimes",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "I Don’t Care",
                    Length = 178
                  },
                  new Song
                  {
                    Title = "Bad Decisions",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "Touch It",
                    Length = 260
                  },
                  new Song
                  {
                    Title = "Knew Better / Forever Boy",
                    Length = 299
                  },
                  new Song
                  {
                    Title = "Thinking Bout You",
                    Length = 200
                  }
                }
              },
              new Album
              {
                Id = 19,
                Title = "Daydream",
                Artist = "Mariah Carey",
                ImageURL = "http://coverartarchive.org/release/68768480-08fa-4593-886c-c03ad0a14234/13352889205-500.jpg",
                Rating = 4.75d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Fantasy",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "Underneath the Stars",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "One Sweet Day",
                    Length = 282
                  },
                  new Song
                  {
                    Title = "Open Arms",
                    Length = 209
                  },
                  new Song
                  {
                    Title = "Always Be My Baby",
                    Length = 259
                  },
                  new Song
                  {
                    Title = "I Am Free",
                    Length = 189
                  },
                  new Song
                  {
                    Title = "When I Saw You",
                    Length = 264
                  },
                  new Song
                  {
                    Title = "Long Ago",
                    Length = 273
                  },
                  new Song
                  {
                    Title = "Melt Away",
                    Length = 222
                  },
                  new Song
                  {
                    Title = "Forever",
                    Length = 240
                  },
                  new Song
                  {
                    Title = "Daydream Interlude (Fantasy Sweet dub mix)",
                    Length = 184
                  },
                  new Song
                  {
                    Title = "Looking In",
                    Length = 215
                  }
                }
              },
              new Album
              {
                Id = 20,
                Title = "Feels Like Home",
                Artist = "Norah Jones",
                ImageURL = "http://coverartarchive.org/release/cbc97377-05f3-4011-a8fa-0aaa1ca9ffd1/22040930793-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Sunrise",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "What Am I to You?",
                    Length = 209
                  },
                  new Song
                  {
                    Title = "Those Sweet Words",
                    Length = 202
                  },
                  new Song
                  {
                    Title = "Carnival Town",
                    Length = 191
                  },
                  new Song
                  {
                    Title = "In the Morning",
                    Length = 247
                  },
                  new Song
                  {
                    Title = "Be Here to Love Me",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "Creepin’ In",
                    Length = 183
                  },
                  new Song
                  {
                    Title = "Toes",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "Humble Me",
                    Length = 276
                  },
                  new Song
                  {
                    Title = "Above Ground",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "The Long Way Home",
                    Length = 193
                  },
                  new Song
                  {
                    Title = "The Prettiest Thing",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Don’t Miss You at All",
                    Length = 188
                  }
                }
              },
              new Album
              {
                Id = 21,
                Title = "FutureSex/LoveSounds",
                Artist = "Justin Timberlake",
                ImageURL = "http://coverartarchive.org/release/73b387fb-e4f4-402b-b684-87319e1191e2/25761925657-500.jpg",
                Rating = 3.9d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "FutureSex / LoveSound",
                    Length = 241
                  },
                  new Song
                  {
                    Title = "SexyBack",
                    Length = 242
                  },
                  new Song
                  {
                    Title = "SexyBack Remix",
                    Length = 334
                  },
                  new Song
                  {
                    Title = "Sexy Ladies / Let Me Talk to You (prelude)",
                    Length = 332
                  },
                  new Song
                  {
                    Title = "My Love",
                    Length = 276
                  },
                  new Song
                  {
                    Title = "LoveStoned / I Think She Knows (interlude)",
                    Length = 444
                  },
                  new Song
                  {
                    Title = "What Goes Around…/…Comes Around (interlude)",
                    Length = 448
                  },
                  new Song
                  {
                    Title = "Damn Girl",
                    Length = 312
                  },
                  new Song
                  {
                    Title = "Summer Love / Set the Mood (prelude)",
                    Length = 384
                  },
                  new Song
                  {
                    Title = "Until the End of Time",
                    Length = 322
                  },
                  new Song
                  {
                    Title = "Losing My Way",
                    Length = 322
                  },
                  new Song
                  {
                    Title = "(Another Song) All Over Again",
                    Length = 345
                  },
                  new Song
                  {
                    Title = "Pose",
                    Length = 286
                  },
                  new Song
                  {
                    Title = "Chop Me Up",
                    Length = 304
                  }
                }
              },
              new Album
              {
                Id = 22,
                Title = "Ghost Stories",
                Artist = "Coldplay",
                ImageURL = "http://coverartarchive.org/release/49dab146-5393-4686-bb79-efbb1fa43648/22395430275-500.jpg",
                Rating = 3.95d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Always in My Head",
                    Length = 216
                  },
                  new Song
                  {
                    Title = "Magic",
                    Length = 285
                  },
                  new Song
                  {
                    Title = "Ink",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "True Love",
                    Length = 246
                  },
                  new Song
                  {
                    Title = "Midnight",
                    Length = 294
                  },
                  new Song
                  {
                    Title = "Another’s Arms",
                    Length = 234
                  },
                  new Song
                  {
                    Title = "Oceans",
                    Length = 321
                  },
                  new Song
                  {
                    Title = "A Sky Full of Stars",
                    Length = 268
                  },
                  new Song
                  {
                    Title = "O",
                    Length = 466
                  }
                }
              },
              new Album
              {
                Id = 23,
                Title = "Hell Freezes Over",
                Artist = "Eagles",
                ImageURL = "http://coverartarchive.org/release/8958944d-cde6-4209-a5cb-539d75421765/13908977441-500.jpg",
                Rating = 3.6d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Get Over It",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Love Will Keep Us Alive",
                    Length = 243
                  },
                  new Song
                  {
                    Title = "The Girl From Yesterday",
                    Length = 203
                  },
                  new Song
                  {
                    Title = "Learn to Be Still",
                    Length = 268
                  },
                  new Song
                  {
                    Title = "Tequila Sunrise",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "Hotel California",
                    Length = 432
                  },
                  new Song
                  {
                    Title = "Wasted Time",
                    Length = 319
                  },
                  new Song
                  {
                    Title = "Pretty Maids All in a Row",
                    Length = 266
                  },
                  new Song
                  {
                    Title = "I Can’t Tell You Why",
                    Length = 311
                  },
                  new Song
                  {
                    Title = "New York Minute",
                    Length = 397
                  },
                  new Song
                  {
                    Title = "The Last Resort",
                    Length = 444
                  },
                  new Song
                  {
                    Title = "Take It Easy",
                    Length = 276
                  },
                  new Song
                  {
                    Title = "In the City",
                    Length = 247
                  },
                  new Song
                  {
                    Title = "Life in the Fast Lane",
                    Length = 361
                  },
                  new Song
                  {
                    Title = "Desperado",
                    Length = 256
                  }
                }
              },
              new Album
              {
                Id = 24,
                Title = "Hourglass",
                Artist = "James Taylor",
                ImageURL = "http://coverartarchive.org/release/61af3776-ce20-40c5-ae23-ac1dc73ddcfe/16411561144-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Line ’em Up",
                    Length = 284
                  },
                  new Song
                  {
                    Title = "Enough to Be on Your Way",
                    Length = 329
                  },
                  new Song
                  {
                    Title = "Little More Time With You",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Gaia",
                    Length = 331
                  },
                  new Song
                  {
                    Title = "Ananas",
                    Length = 345
                  },
                  new Song
                  {
                    Title = "Jump Up Behind Me",
                    Length = 210
                  },
                  new Song
                  {
                    Title = "Another Day",
                    Length = 142
                  },
                  new Song
                  {
                    Title = "Up Er Mei",
                    Length = 229
                  },
                  new Song
                  {
                    Title = "Up From Your Life",
                    Length = 317
                  },
                  new Song
                  {
                    Title = "Yellow and Rose",
                    Length = 295
                  },
                  new Song
                  {
                    Title = "Boatman",
                    Length = 239
                  },
                  new Song
                  {
                    Title = "Walking My Baby Back Home",
                    Length = 193
                  },
                  new Song
                  {
                    Title = "Hangnail",
                    Length = 142
                  }
                }
              },
              new Album
              {
                Id = 25,
                Title = "I Dreamed a Dream",
                Artist = "Susan Boyle",
                ImageURL = "http://coverartarchive.org/release/5e5f9480-5a26-3136-b66f-b63569abeea2/26591638014-500.jpg",
                Rating = 3.35d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Wild Horses",
                    Length = 295
                  },
                  new Song
                  {
                    Title = "I Dreamed a Dream",
                    Length = 190
                  },
                  new Song
                  {
                    Title = "Cry Me a River",
                    Length = 162
                  },
                  new Song
                  {
                    Title = "How Great Thou Art",
                    Length = 192
                  },
                  new Song
                  {
                    Title = "You’ll See",
                    Length = 282
                  },
                  new Song
                  {
                    Title = "Daydream Believer",
                    Length = 199
                  },
                  new Song
                  {
                    Title = "Up to the Mountain",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Amazing Grace",
                    Length = 215
                  },
                  new Song
                  {
                    Title = "Who I Was Born to Be",
                    Length = 250
                  },
                  new Song
                  {
                    Title = "Proud",
                    Length = 202
                  },
                  new Song
                  {
                    Title = "The End of the World",
                    Length = 195
                  },
                  new Song
                  {
                    Title = "Silent Night",
                    Length = 179
                  }
                }
              },
              new Album
              {
                Id = 26,
                Title = "I Love Everybody",
                Artist = "Lyle Lovett",
                ImageURL = "http://coverartarchive.org/release/aea36fb5-706f-4bc7-bdb8-b3a98614afe3/19897925140-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Skinny Legs",
                    Length = 163
                  },
                  new Song
                  {
                    Title = "Fat Babies",
                    Length = 173
                  },
                  new Song
                  {
                    Title = "I Think You Know What I Mean",
                    Length = 185
                  },
                  new Song
                  {
                    Title = "Hello Grandma",
                    Length = 155
                  },
                  new Song
                  {
                    Title = "Creeps Like Me",
                    Length = 134
                  },
                  new Song
                  {
                    Title = "Sonja",
                    Length = 120
                  },
                  new Song
                  {
                    Title = "They Don’t Like Me",
                    Length = 154
                  },
                  new Song
                  {
                    Title = "Record Lady",
                    Length = 251
                  },
                  new Song
                  {
                    Title = "Ain’t It Somethin’",
                    Length = 134
                  },
                  new Song
                  {
                    Title = "Penguins",
                    Length = 151
                  },
                  new Song
                  {
                    Title = "The Fat Girl",
                    Length = 120
                  },
                  new Song
                  {
                    Title = "La to the Left",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "Old Friend",
                    Length = 188
                  },
                  new Song
                  {
                    Title = "Just the Morning",
                    Length = 263
                  },
                  new Song
                  {
                    Title = "Moon on My Shoulder",
                    Length = 140
                  },
                  new Song
                  {
                    Title = "I’ve Got the Blues",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "Good‐bye to Carolina",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "I Love Everybody",
                    Length = 217
                  }
                }
              },
              new Album
              {
                Id = 27,
                Title = "Inside Job",
                Artist = "Don Henley",
                ImageURL = "http://coverartarchive.org/release/62c0a78c-9b48-431a-9b78-3b7a945da22b/27681252321-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Nobody Else in the World but You",
                    Length = 290
                  },
                  new Song
                  {
                    Title = "Taking You Home",
                    Length = 331
                  },
                  new Song
                  {
                    Title = "For My Wedding",
                    Length = 217
                  },
                  new Song
                  {
                    Title = "Everything Is Different Now",
                    Length = 313
                  },
                  new Song
                  {
                    Title = "Workin’ It",
                    Length = 337
                  },
                  new Song
                  {
                    Title = "Goodbye to a River",
                    Length = 349
                  },
                  new Song
                  {
                    Title = "Inside Job",
                    Length = 290
                  },
                  new Song
                  {
                    Title = "They’re Not Here, They’re Not Coming",
                    Length = 359
                  },
                  new Song
                  {
                    Title = "Damn It, Rose",
                    Length = 433
                  },
                  new Song
                  {
                    Title = "Miss Ghost",
                    Length = 401
                  },
                  new Song
                  {
                    Title = "The Genie",
                    Length = 345
                  },
                  new Song
                  {
                    Title = "Annabel",
                    Length = 221
                  },
                  new Song
                  {
                    Title = "My Thanksgiving",
                    Length = 312
                  }
                }
              },
              new Album
              {
                Id = 28,
                Title = "It Won’t Be Soon Before Long",
                Artist = "Maroon 5",
                ImageURL = "http://coverartarchive.org/release/03f96a62-44ba-4d84-b4b0-a1718e1aedef/27905595314-500.jpg",
                Rating = 3.55d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "If I Never See Your Face Again",
                    Length = 201
                  },
                  new Song
                  {
                    Title = "Makes Me Wonder (album version)",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Little of Your Time",
                    Length = 137
                  },
                  new Song
                  {
                    Title = "Wake Up Call",
                    Length = 201
                  },
                  new Song
                  {
                    Title = "Won’t Go Home Without You",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Nothing Lasts Forever",
                    Length = 187
                  },
                  new Song
                  {
                    Title = "Can’t Stop",
                    Length = 152
                  },
                  new Song
                  {
                    Title = "Goodnight Goodnight",
                    Length = 243
                  },
                  new Song
                  {
                    Title = "Not Falling Apart",
                    Length = 243
                  },
                  new Song
                  {
                    Title = "Kiwi",
                    Length = 214
                  },
                  new Song
                  {
                    Title = "Better That We Break",
                    Length = 186
                  },
                  new Song
                  {
                    Title = "Back at Your Door",
                    Length = 227
                  }
                }
              },
              new Album
              {
                Id = 29,
                Title = "Joanne",
                Artist = "Lady Gaga",
                ImageURL = "http://coverartarchive.org/release/fe610bb2-88e1-4ce8-a723-19d0b16c3b01/14716160882-500.jpg",
                Rating = 3.8d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Diamond Heart",
                    Length = 210
                  },
                  new Song
                  {
                    Title = "A‐YO",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Joanne",
                    Length = 196
                  },
                  new Song
                  {
                    Title = "John Wayne",
                    Length = 174
                  },
                  new Song
                  {
                    Title = "Dancin’ in Circles",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Perfect Illusion",
                    Length = 182
                  },
                  new Song
                  {
                    Title = "Million Reasons",
                    Length = 205
                  },
                  new Song
                  {
                    Title = "Sinner’s Prayer",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "Come to Mama",
                    Length = 254
                  },
                  new Song
                  {
                    Title = "Hey Girl",
                    Length = 255
                  },
                  new Song
                  {
                    Title = "Angel Down",
                    Length = 229
                  }
                }
              },
              new Album
              {
                Id = 30,
                Title = "Justice",
                Artist = "Justin Bieber",
                ImageURL = "http://coverartarchive.org/release/9bd9e1ae-00e3-48fa-bd90-c6e2120893f3/28752595089-500.jpg",
                Rating = 5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "2 Much",
                    Length = 152
                  },
                  new Song
                  {
                    Title = "Deserve You",
                    Length = 187
                  },
                  new Song
                  {
                    Title = "As I Am",
                    Length = 174
                  },
                  new Song
                  {
                    Title = "Off My Face",
                    Length = 156
                  },
                  new Song
                  {
                    Title = "Holy",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "Unstable",
                    Length = 158
                  },
                  new Song
                  {
                    Title = "MLK Interlude",
                    Length = 104
                  },
                  new Song
                  {
                    Title = "Die for You",
                    Length = 198
                  },
                  new Song
                  {
                    Title = "Hold On",
                    Length = 170
                  },
                  new Song
                  {
                    Title = "Somebody",
                    Length = 179
                  },
                  new Song
                  {
                    Title = "Ghost",
                    Length = 153
                  },
                  new Song
                  {
                    Title = "Peaches",
                    Length = 198
                  },
                  new Song
                  {
                    Title = "Love You Different",
                    Length = 186
                  },
                  new Song
                  {
                    Title = "Loved by You",
                    Length = 159
                  },
                  new Song
                  {
                    Title = "Anyone",
                    Length = 190
                  },
                  new Song
                  {
                    Title = "Lonely",
                    Length = 149
                  }
                }
              },
              new Album
              {
                Id = 31,
                Title = "Left of the Middle",
                Artist = "Natalie Imbruglia",
                ImageURL = "http://coverartarchive.org/release/47aac2c4-0b17-4a71-963b-c0dd3c540750/31136848115-500.jpg",
                Rating = 4.2d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Torn",
                    Length = 245
                  },
                  new Song
                  {
                    Title = "One More Addiction",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Big Mistake",
                    Length = 273
                  },
                  new Song
                  {
                    Title = "Leave Me Alone",
                    Length = 263
                  },
                  new Song
                  {
                    Title = "Wishing I Was There",
                    Length = 232
                  },
                  new Song
                  {
                    Title = "Smoke",
                    Length = 277
                  },
                  new Song
                  {
                    Title = "Pigeons and Crumbs",
                    Length = 321
                  },
                  new Song
                  {
                    Title = "Don\'t You Think?",
                    Length = 235
                  },
                  new Song
                  {
                    Title = "Impressed",
                    Length = 288
                  },
                  new Song
                  {
                    Title = "Intuition",
                    Length = 203
                  },
                  new Song
                  {
                    Title = "City / [silence]",
                    Length = 574
                  },
                  new Song
                  {
                    Title = "Left of the Middle",
                    Length = 227
                  }
                }
              },
              new Album
              {
                Id = 32,
                Title = "Let Go",
                Artist = "Avril Lavigne",
                ImageURL = "http://coverartarchive.org/release/c2caa2a9-a3f3-4452-a7de-34ba4072b7f7/27602931165-500.jpg",
                Rating = 4.1d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Losing Grip",
                    Length = 233
                  },
                  new Song
                  {
                    Title = "Complicated",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "Sk8er Boi",
                    Length = 204
                  },
                  new Song
                  {
                    Title = "I’m With You",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "Mobile",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Unwanted",
                    Length = 221
                  },
                  new Song
                  {
                    Title = "Tomorrow",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "Anything but Ordinary",
                    Length = 251
                  },
                  new Song
                  {
                    Title = "Things I’ll Never Say",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "My World",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Nobody’s Fool",
                    Length = 237
                  },
                  new Song
                  {
                    Title = "Too Much to Ask",
                    Length = 225
                  },
                  new Song
                  {
                    Title = "Naked",
                    Length = 208
                  }
                }
              },
              new Album
              {
                Id = 33,
                Title = "Let’s Talk About Love",
                Artist = "Céline Dion",
                ImageURL = "http://coverartarchive.org/release/c14ff282-311f-41a9-b7d7-d4f4abb3869a/14611896642-500.jpg",
                Rating = 4.15d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "The Reason",
                    Length = 301
                  },
                  new Song
                  {
                    Title = "Immortality",
                    Length = 251
                  },
                  new Song
                  {
                    Title = "Treat Her Like a Lady",
                    Length = 245
                  },
                  new Song
                  {
                    Title = "Why Oh Why",
                    Length = 291
                  },
                  new Song
                  {
                    Title = "Love Is on the Way",
                    Length = 266
                  },
                  new Song
                  {
                    Title = "Tell Him",
                    Length = 292
                  },
                  new Song
                  {
                    Title = "Where Is the Love",
                    Length = 295
                  },
                  new Song
                  {
                    Title = "When I Need You",
                    Length = 252
                  },
                  new Song
                  {
                    Title = "Miles to Go (Before I Sleep)",
                    Length = 279
                  },
                  new Song
                  {
                    Title = "Us",
                    Length = 346
                  },
                  new Song
                  {
                    Title = "Just a Little Bit of Love",
                    Length = 246
                  },
                  new Song
                  {
                    Title = "My Heart Will Go On",
                    Length = 280
                  },
                  new Song
                  {
                    Title = "I Hate You Then I Love You",
                    Length = 283
                  },
                  new Song
                  {
                    Title = "To Love You More",
                    Length = 328
                  },
                  new Song
                  {
                    Title = "Let’s Talk About Love",
                    Length = 312
                  }
                }
              },
              new Album
              {
                Id = 34,
                Title = "Long Road Out of Eden",
                Artist = "Eagles",
                ImageURL = "http://coverartarchive.org/release/b97e8e55-471f-3549-8a16-e26d6950d997/5877076684-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "No More Walks in the Wood",
                    Length = 120
                  },
                  new Song
                  {
                    Title = "How Long",
                    Length = 196
                  },
                  new Song
                  {
                    Title = "Busy Being Fabulous",
                    Length = 260
                  },
                  new Song
                  {
                    Title = "What Do I Do With My Heart",
                    Length = 234
                  },
                  new Song
                  {
                    Title = "Guilty of the Crime",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "I Don’t Want to Hear Any More",
                    Length = 261
                  },
                  new Song
                  {
                    Title = "Waiting in the Weeds",
                    Length = 466
                  },
                  new Song
                  {
                    Title = "No More Cloudy Days",
                    Length = 243
                  },
                  new Song
                  {
                    Title = "Fast Company",
                    Length = 240
                  },
                  new Song
                  {
                    Title = "Do Something",
                    Length = 312
                  },
                  new Song
                  {
                    Title = "You Are Not Alone",
                    Length = 144
                  }
                }
              },
              new Album
              {
                Id = 35,
                Title = "Love. Angel. Music. Baby.",
                Artist = "Gwen Stefani",
                ImageURL = "http://coverartarchive.org/release/a6b91c06-e99e-48e3-b39c-7098751de740/31940908283-500.jpg",
                Rating = 3d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "What You Waiting For?",
                    Length = 221
                  },
                  new Song
                  {
                    Title = "Rich Girl",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "Hollaback Girl",
                    Length = 199
                  },
                  new Song
                  {
                    Title = "Cool",
                    Length = 189
                  },
                  new Song
                  {
                    Title = "Bubble Pop Electric",
                    Length = 222
                  },
                  new Song
                  {
                    Title = "Luxurious",
                    Length = 264
                  },
                  new Song
                  {
                    Title = "Harajuku Girls",
                    Length = 291
                  },
                  new Song
                  {
                    Title = "Crash",
                    Length = 246
                  },
                  new Song
                  {
                    Title = "The Real Thing",
                    Length = 251
                  },
                  new Song
                  {
                    Title = "Serious",
                    Length = 288
                  },
                  new Song
                  {
                    Title = "Danger Zone",
                    Length = 217
                  },
                  new Song
                  {
                    Title = "Long Way to Go",
                    Length = 274
                  }
                }
              },
              new Album
              {
                Id = 36,
                Title = "Lust for Life",
                Artist = "Lana Del Rey",
                ImageURL = "http://coverartarchive.org/release/2ea08cad-8599-4fe7-a6e7-baa52b8034b1/17312782925-500.jpg",
                Rating = 3.85d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Love",
                    Length = 272
                  },
                  new Song
                  {
                    Title = "Lust for Life",
                    Length = 264
                  },
                  new Song
                  {
                    Title = "13 Beaches",
                    Length = 295
                  },
                  new Song
                  {
                    Title = "Cherry",
                    Length = 180
                  },
                  new Song
                  {
                    Title = "White Mustang",
                    Length = 164
                  },
                  new Song
                  {
                    Title = "Summer Bummer",
                    Length = 260
                  },
                  new Song
                  {
                    Title = "Groupie Love",
                    Length = 264
                  },
                  new Song
                  {
                    Title = "In My Feelings",
                    Length = 238
                  },
                  new Song
                  {
                    Title = "Coachella — Woodstock in My Mind",
                    Length = 258
                  },
                  new Song
                  {
                    Title = "God Bless America — And All the Beautiful Women in It",
                    Length = 276
                  },
                  new Song
                  {
                    Title = "When the World Was at War We Kept Dancing",
                    Length = 275
                  },
                  new Song
                  {
                    Title = "Beautiful People Beautiful Problems",
                    Length = 253
                  },
                  new Song
                  {
                    Title = "Tomorrow Never Came",
                    Length = 307
                  },
                  new Song
                  {
                    Title = "Heroin",
                    Length = 355
                  },
                  new Song
                  {
                    Title = "Change",
                    Length = 321
                  },
                  new Song
                  {
                    Title = "Get Free",
                    Length = 334
                  }
                }
              },
              new Album
              {
                Id = 37,
                Title = "Meaning of Life",
                Artist = "Kelly Clarkson",
                ImageURL = "http://coverartarchive.org/release/202bec30-e644-4b0a-a0e3-ff8991bc8ee7/18602981044-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "A Minute (intro)",
                    Length = 68
                  },
                  new Song
                  {
                    Title = "Love So Soft",
                    Length = 172
                  },
                  new Song
                  {
                    Title = "Heat",
                    Length = 190
                  },
                  new Song
                  {
                    Title = "Meaning of Life",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Move You",
                    Length = 202
                  },
                  new Song
                  {
                    Title = "Whole Lotta Woman",
                    Length = 173
                  },
                  new Song
                  {
                    Title = "Medicine",
                    Length = 189
                  },
                  new Song
                  {
                    Title = "Cruel",
                    Length = 185
                  },
                  new Song
                  {
                    Title = "Didn’t I",
                    Length = 218
                  },
                  new Song
                  {
                    Title = "Would You Call That Love",
                    Length = 178
                  },
                  new Song
                  {
                    Title = "I Don’t Think About You",
                    Length = 224
                  },
                  new Song
                  {
                    Title = "Slow Dance",
                    Length = 220
                  },
                  new Song
                  {
                    Title = "Don’t You Pretend",
                    Length = 193
                  },
                  new Song
                  {
                    Title = "Go High",
                    Length = 205
                  }
                }
              },
              new Album
              {
                Id = 38,
                Title = "Medusa",
                Artist = "Annie Lennox",
                ImageURL = "http://coverartarchive.org/release/55229cad-2f4f-4b02-8530-7e9d199018fc/21248619657-500.jpg",
                Rating = 3.5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "No More “I Love You’s”",
                    Length = 291
                  },
                  new Song
                  {
                    Title = "Take Me to the River",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "A Whiter Shade of Pale",
                    Length = 318
                  },
                  new Song
                  {
                    Title = "Don’t Let It Bring You Down",
                    Length = 218
                  },
                  new Song
                  {
                    Title = "Train in Vain",
                    Length = 284
                  },
                  new Song
                  {
                    Title = "I Can’t Get Next to You",
                    Length = 188
                  },
                  new Song
                  {
                    Title = "Downtown Lights",
                    Length = 404
                  },
                  new Song
                  {
                    Title = "Thin Line Between Love and Hate",
                    Length = 295
                  },
                  new Song
                  {
                    Title = "Waiting in Vain",
                    Length = 340
                  },
                  new Song
                  {
                    Title = "Something So Right",
                    Length = 235
                  }
                }
              },
              new Album
              {
                Id = 39,
                Title = "Memory Almost Full",
                Artist = "Paul McCartney",
                ImageURL = "http://coverartarchive.org/release/78b8e7bd-6f5a-46c4-96c1-705e50c697a7/21009905624-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Dance Tonight",
                    Length = 174
                  },
                  new Song
                  {
                    Title = "Ever Present Past",
                    Length = 177
                  },
                  new Song
                  {
                    Title = "See Your Sunshine",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "Only Mama Knows",
                    Length = 257
                  },
                  new Song
                  {
                    Title = "You Tell Me",
                    Length = 195
                  },
                  new Song
                  {
                    Title = "Mr. Bellamy",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "Gratitude",
                    Length = 199
                  },
                  new Song
                  {
                    Title = "Vintage Clothes",
                    Length = 142
                  },
                  new Song
                  {
                    Title = "That Was Me",
                    Length = 158
                  },
                  new Song
                  {
                    Title = "Feet in the Clouds",
                    Length = 204
                  },
                  new Song
                  {
                    Title = "House of Wax",
                    Length = 299
                  },
                  new Song
                  {
                    Title = "The End of the End",
                    Length = 177
                  },
                  new Song
                  {
                    Title = "Nod Your Head",
                    Length = 118
                  }
                }
              },
              new Album
              {
                Id = 40,
                Title = "Mercury Falling",
                Artist = "Sting",
                ImageURL = "http://coverartarchive.org/release/7148f1f0-012b-4fb3-97e8-efb9fe0433dd/13915422169-500.jpg",
                Rating = 3.5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "The Hounds of Winter",
                    Length = 326
                  },
                  new Song
                  {
                    Title = "I Hung My Head",
                    Length = 280
                  },
                  new Song
                  {
                    Title = "Let Your Soul Be Your Pilot",
                    Length = 403
                  },
                  new Song
                  {
                    Title = "I Was Brought to My Senses",
                    Length = 350
                  },
                  new Song
                  {
                    Title = "You Still Touch Me",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "I’m So Happy I Can’t Stop Crying",
                    Length = 237
                  },
                  new Song
                  {
                    Title = "All Four Seasons",
                    Length = 268
                  },
                  new Song
                  {
                    Title = "La Belle Dame sans regrets",
                    Length = 316
                  },
                  new Song
                  {
                    Title = "Valparaiso",
                    Length = 327
                  },
                  new Song
                  {
                    Title = "Lithium Sunset",
                    Length = 157
                  }
                }
              },
              new Album
              {
                Id = 41,
                Title = "Millennium",
                Artist = "Backstreet Boys",
                ImageURL = "http://coverartarchive.org/release/6f7d0a0e-2c47-4997-b9d8-4d6912c2aa58/24967179857-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Larger Than Life",
                    Length = 233
                  },
                  new Song
                  {
                    Title = "I Want It That Way",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "Show Me the Meaning of Being Lonely",
                    Length = 235
                  },
                  new Song
                  {
                    Title = "It’s Gotta Be You",
                    Length = 177
                  },
                  new Song
                  {
                    Title = "I Need You Tonight",
                    Length = 263
                  },
                  new Song
                  {
                    Title = "Don’t Want You Back",
                    Length = 206
                  },
                  new Song
                  {
                    Title = "Don’t Wanna Lose You Now",
                    Length = 235
                  },
                  new Song
                  {
                    Title = "The One (album version)",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "Back to Your Heart",
                    Length = 261
                  },
                  new Song
                  {
                    Title = "Spanish Eyes",
                    Length = 235
                  },
                  new Song
                  {
                    Title = "No One Else Comes Close",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "The Perfect Fan",
                    Length = 253
                  }
                }
              },
              new Album
              {
                Id = 42,
                Title = "Motown",
                Artist = "Michael McDonald",
                ImageURL = "http://coverartarchive.org/release/8a16689d-e390-41de-a937-e748427020b4/31626708829-500.jpg",
                Rating = 3.4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "I Heard It Through the Grapevine",
                    Length = 224
                  },
                  new Song
                  {
                    Title = "You Are Everything",
                    Length = 168
                  },
                  new Song
                  {
                    Title = "Signed, Sealed, Delivered I’m Yours",
                    Length = 247
                  },
                  new Song
                  {
                    Title = "I’m Gonna Make You Love Me",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "Ain’t Nothing Like the Real Thing",
                    Length = 168
                  },
                  new Song
                  {
                    Title = "Reflections",
                    Length = 202
                  },
                  new Song
                  {
                    Title = "How Sweet It Is (to Be Loved by You)",
                    Length = 321
                  },
                  new Song
                  {
                    Title = "Ain’t No Mountain High Enough",
                    Length = 169
                  },
                  new Song
                  {
                    Title = "All in Love Is Fair",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "I Want You",
                    Length = 267
                  },
                  new Song
                  {
                    Title = "Distant Lover",
                    Length = 260
                  },
                  new Song
                  {
                    Title = "I Believe (When I Fall in Love It Will Be Forever)",
                    Length = 305
                  },
                  new Song
                  {
                    Title = "Since I Lost My Baby",
                    Length = 274
                  },
                  new Song
                  {
                    Title = "Too High",
                    Length = 325
                  }
                }
              },
              new Album
              {
                Id = 43,
                Title = "My Everything",
                Artist = "Ariana Grande",
                ImageURL = "http://coverartarchive.org/release/17403b4c-ce83-47a5-a932-763894d91e3c/20706562198-500.jpg",
                Rating = 4.5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Intro",
                    Length = 79
                  },
                  new Song
                  {
                    Title = "Problem",
                    Length = 193
                  },
                  new Song
                  {
                    Title = "One Last Time",
                    Length = 197
                  },
                  new Song
                  {
                    Title = "Why Try",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Break Free",
                    Length = 214
                  },
                  new Song
                  {
                    Title = "Best Mistake",
                    Length = 233
                  },
                  new Song
                  {
                    Title = "Be My Baby",
                    Length = 217
                  },
                  new Song
                  {
                    Title = "Break Your Heart Right Back",
                    Length = 253
                  },
                  new Song
                  {
                    Title = "Love Me Harder",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "Just a Little Bit of Your Heart",
                    Length = 232
                  },
                  new Song
                  {
                    Title = "Hands on Me",
                    Length = 192
                  },
                  new Song
                  {
                    Title = "My Everything",
                    Length = 168
                  },
                  new Song
                  {
                    Title = "Bang Bang",
                    Length = 199
                  },
                  new Song
                  {
                    Title = "Only 1",
                    Length = 193
                  },
                  new Song
                  {
                    Title = "You Don’t Know Me",
                    Length = 233
                  }
                }
              },
              new Album
              {
                Id = 44,
                Title = "New Beginning",
                Artist = "Tracy Chapman",
                ImageURL = "http://coverartarchive.org/release/73d50f5a-ce08-31d1-a1f5-9ad9387e501f/27897369197-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Heaven’s Here on Earth",
                    Length = 323
                  },
                  new Song
                  {
                    Title = "New Beginning",
                    Length = 333
                  },
                  new Song
                  {
                    Title = "Smoke and Ashes",
                    Length = 399
                  },
                  new Song
                  {
                    Title = "Cold Feet",
                    Length = 340
                  },
                  new Song
                  {
                    Title = "At This Point in My Life",
                    Length = 309
                  },
                  new Song
                  {
                    Title = "The Promise",
                    Length = 328
                  },
                  new Song
                  {
                    Title = "The Rape of the World",
                    Length = 430
                  },
                  new Song
                  {
                    Title = "Tell It Like It Is",
                    Length = 365
                  },
                  new Song
                  {
                    Title = "Give Me One Reason",
                    Length = 270
                  },
                  new Song
                  {
                    Title = "Remember the Tinman",
                    Length = 345
                  },
                  new Song
                  {
                    Title = "I’m Ready",
                    Length = 428
                  }
                }
              },
              new Album
              {
                Id = 45,
                Title = "No Strings Attached",
                Artist = "*NSYNC",
                ImageURL = "http://coverartarchive.org/release/37ad75f2-571d-498c-84d4-07327d3d51a8/31107205240-500.jpg",
                Rating = 3.6d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Bye Bye Bye",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "It’s Gonna Be Me",
                    Length = 192
                  },
                  new Song
                  {
                    Title = "Space Cowboy (Yippie‐Yi‐Yay)",
                    Length = 263
                  },
                  new Song
                  {
                    Title = "Just Got Paid",
                    Length = 250
                  },
                  new Song
                  {
                    Title = "It Makes Me Ill",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "This I Promise You",
                    Length = 283
                  },
                  new Song
                  {
                    Title = "No Strings Attached",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "Digital Get Down",
                    Length = 263
                  },
                  new Song
                  {
                    Title = "Bringin’ da Noise",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "That’s When I’ll Stop Loving You",
                    Length = 291
                  },
                  new Song
                  {
                    Title = "I’ll Be Good for You",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "I Thought She Knew",
                    Length = 202
                  }
                }
              },
              new Album
              {
                Id = 46,
                Title = "No.6 Collaborations Project",
                Artist = "Ed Sheeran",
                ImageURL = "http://coverartarchive.org/release/503ddbc4-4564-42fd-8443-0e2b6fbf325a/23613938478-500.jpg",
                Rating = 2d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Beautiful People",
                    Length = 197
                  },
                  new Song
                  {
                    Title = "South of the Border",
                    Length = 204
                  },
                  new Song
                  {
                    Title = "Cross Me",
                    Length = 206
                  },
                  new Song
                  {
                    Title = "Take Me Back to London",
                    Length = 189
                  },
                  new Song
                  {
                    Title = "Best Part of Me",
                    Length = 243
                  },
                  new Song
                  {
                    Title = "I Don’t Care",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "Antisocial",
                    Length = 161
                  },
                  new Song
                  {
                    Title = "Remember the Name",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Feels",
                    Length = 150
                  },
                  new Song
                  {
                    Title = "Put It All on Me",
                    Length = 197
                  },
                  new Song
                  {
                    Title = "Nothing on You",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "I Don’t Want Your Money",
                    Length = 204
                  },
                  new Song
                  {
                    Title = "1000 Nights",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "Way to Break My Heart",
                    Length = 190
                  },
                  new Song
                  {
                    Title = "BLOW",
                    Length = 209
                  }
                }
              },
              new Album
              {
                Id = 47,
                Title = "Ode to Billie Joe",
                Artist = "Bobbie Gentry",
                ImageURL = "http://coverartarchive.org/release/2b97fbfe-1282-44da-a2b2-a927a2801826/9246100437-500.jpg",
                Rating = 4.4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Mississippi Delta",
                    Length = 185
                  },
                  new Song
                  {
                    Title = "I Saw an Angel Die",
                    Length = 177
                  },
                  new Song
                  {
                    Title = "Chickasaw County Child",
                    Length = 167
                  },
                  new Song
                  {
                    Title = "Sunday Best",
                    Length = 164
                  },
                  new Song
                  {
                    Title = "Niki Hoeky",
                    Length = 166
                  },
                  new Song
                  {
                    Title = "Papa, Won’t You Let Me Go to Town With You",
                    Length = 154
                  },
                  new Song
                  {
                    Title = "Bugs",
                    Length = 129
                  },
                  new Song
                  {
                    Title = "Hurry, Tuesday Child",
                    Length = 234
                  },
                  new Song
                  {
                    Title = "Lazy Willie",
                    Length = 161
                  },
                  new Song
                  {
                    Title = "Ode to Billy Joe",
                    Length = 254
                  }
                }
              },
              new Album
              {
                Id = 48,
                Title = "Oops!…I Did It Again",
                Artist = "Britney Spears",
                ImageURL = "http://coverartarchive.org/release/b51fc656-4df6-492f-a3f3-977c78305965/28973453342-500.jpg",
                Rating = 3.75d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Oops!…I Did It Again",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Stronger",
                    Length = 203
                  },
                  new Song
                  {
                    Title = "Don’t Go Knockin’ on My Door",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "(I Can’t Get No) Satisfaction",
                    Length = 265
                  },
                  new Song
                  {
                    Title = "Don’t Let Me Be the Last to Know",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "What U See (Is What U Get)",
                    Length = 216
                  },
                  new Song
                  {
                    Title = "Lucky",
                    Length = 206
                  },
                  new Song
                  {
                    Title = "One Kiss From You",
                    Length = 205
                  },
                  new Song
                  {
                    Title = "Where Are You Now",
                    Length = 279
                  },
                  new Song
                  {
                    Title = "Can’t Make You Love Me",
                    Length = 197
                  },
                  new Song
                  {
                    Title = "When Your Eyes Say It",
                    Length = 269
                  },
                  new Song
                  {
                    Title = "Dear Diary",
                    Length = 166
                  }
                }
              },
              new Album
              {
                Id = 49,
                Title = "Paradise",
                Artist = "Lana Del Rey",
                ImageURL = "http://coverartarchive.org/release/a4019600-2177-45b4-b83c-aaedcda957a1/28044847830-500.jpg",
                Rating = 3.85d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Ride",
                    Length = 289
                  },
                  new Song
                  {
                    Title = "American",
                    Length = 248
                  },
                  new Song
                  {
                    Title = "Cola",
                    Length = 260
                  },
                  new Song
                  {
                    Title = "Body Electric",
                    Length = 233
                  },
                  new Song
                  {
                    Title = "Blue Velvet",
                    Length = 158
                  },
                  new Song
                  {
                    Title = "Gods and Monsters",
                    Length = 237
                  },
                  new Song
                  {
                    Title = "Yayo",
                    Length = 321
                  },
                  new Song
                  {
                    Title = "Bel Air",
                    Length = 237
                  }
                }
              },
              new Album
              {
                Id = 50,
                Title = "Pilgrim",
                Artist = "Eric Clapton",
                ImageURL = "http://coverartarchive.org/release/860211e1-fda7-4f25-8ac1-203769a3064f/22965004617-500.jpg",
                Rating = 3d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "My Father’s Eyes",
                    Length = 324
                  },
                  new Song
                  {
                    Title = "River of Tears",
                    Length = 442
                  },
                  new Song
                  {
                    Title = "Pilgrim",
                    Length = 350
                  },
                  new Song
                  {
                    Title = "Broken Hearted",
                    Length = 472
                  },
                  new Song
                  {
                    Title = "One Chance",
                    Length = 355
                  },
                  new Song
                  {
                    Title = "Circus",
                    Length = 251
                  },
                  new Song
                  {
                    Title = "Going Down Slow",
                    Length = 319
                  },
                  new Song
                  {
                    Title = "Fall Like Rain",
                    Length = 230
                  },
                  new Song
                  {
                    Title = "Born in Time",
                    Length = 281
                  },
                  new Song
                  {
                    Title = "Sick and Tired",
                    Length = 343
                  },
                  new Song
                  {
                    Title = "Needs His Woman",
                    Length = 225
                  },
                  new Song
                  {
                    Title = "She’s Gone",
                    Length = 285
                  },
                  new Song
                  {
                    Title = "You Were There",
                    Length = 331
                  },
                  new Song
                  {
                    Title = "Inside of Me",
                    Length = 325
                  }
                }
              },
              new Album
              {
                Id = 51,
                Title = "Planet Her",
                Artist = "Doja Cat",
                ImageURL = "http://coverartarchive.org/release/d1907623-576f-48cb-b413-add073ad6374/31476560975-500.jpg",
                Rating = 3d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Woman",
                    Length = 172
                  },
                  new Song
                  {
                    Title = "Naked",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "Payday",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "Get Into It (Yuh)",
                    Length = 138
                  },
                  new Song
                  {
                    Title = "Need to Know",
                    Length = 210
                  },
                  new Song
                  {
                    Title = "I Don’t Do Drugs",
                    Length = 188
                  },
                  new Song
                  {
                    Title = "Love to Dream",
                    Length = 216
                  },
                  new Song
                  {
                    Title = "You Right",
                    Length = 186
                  },
                  new Song
                  {
                    Title = "Been Like This",
                    Length = 177
                  },
                  new Song
                  {
                    Title = "Options",
                    Length = 159
                  },
                  new Song
                  {
                    Title = "Ain’t Shit",
                    Length = 174
                  },
                  new Song
                  {
                    Title = "Imagine",
                    Length = 148
                  },
                  new Song
                  {
                    Title = "Alone",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "Kiss Me More",
                    Length = 208
                  }
                }
              },
              new Album
              {
                Id = 52,
                Title = "Positions",
                Artist = "Ariana Grande",
                ImageURL = "http://coverartarchive.org/release/cfef369e-5366-4a02-8f13-1f5d241d6f69/29222626508-500.jpg",
                Rating = 4.6d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "shut up",
                    Length = 157
                  },
                  new Song
                  {
                    Title = "34+35",
                    Length = 173
                  },
                  new Song
                  {
                    Title = "motive",
                    Length = 167
                  },
                  new Song
                  {
                    Title = "just like magic",
                    Length = 149
                  },
                  new Song
                  {
                    Title = "off the table",
                    Length = 239
                  },
                  new Song
                  {
                    Title = "six thirty",
                    Length = 183
                  },
                  new Song
                  {
                    Title = "safety net",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "my hair",
                    Length = 158
                  },
                  new Song
                  {
                    Title = "nasty",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "west side",
                    Length = 132
                  },
                  new Song
                  {
                    Title = "love language",
                    Length = 179
                  },
                  new Song
                  {
                    Title = "positions",
                    Length = 172
                  },
                  new Song
                  {
                    Title = "obvious",
                    Length = 146
                  },
                  new Song
                  {
                    Title = "pov",
                    Length = 201
                  }
                }
              },
              new Album
              {
                Id = 53,
                Title = "PRISM",
                Artist = "Katy Perry",
                ImageURL = "http://coverartarchive.org/release/05a39738-e056-4504-8727-f1728c03dee8/32476621742-500.jpg",
                Rating = 4.25d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Roar",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "Legendary Lovers",
                    Length = 224
                  },
                  new Song
                  {
                    Title = "Birthday",
                    Length = 215
                  },
                  new Song
                  {
                    Title = "Walking on Air",
                    Length = 222
                  },
                  new Song
                  {
                    Title = "Unconditionally",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "Dark Horse",
                    Length = 215
                  },
                  new Song
                  {
                    Title = "This Is How We Do",
                    Length = 204
                  },
                  new Song
                  {
                    Title = "International Smile",
                    Length = 227
                  },
                  new Song
                  {
                    Title = "Ghost",
                    Length = 203
                  },
                  new Song
                  {
                    Title = "Love Me",
                    Length = 232
                  },
                  new Song
                  {
                    Title = "This Moment",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "Double Rainbow",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "By the Grace of God",
                    Length = 267
                  }
                }
              },
              new Album
              {
                Id = 54,
                Title = "Purpose",
                Artist = "Justin Bieber",
                ImageURL = "http://coverartarchive.org/release/4eaf0312-83a6-4cc8-8c8d-1e2bc365ea36/27465786957-500.jpg",
                Rating = 3.7d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Mark My Words",
                    Length = 134
                  },
                  new Song
                  {
                    Title = "I’ll Show You",
                    Length = 199
                  },
                  new Song
                  {
                    Title = "What Do You Mean?",
                    Length = 205
                  },
                  new Song
                  {
                    Title = "Sorry",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "Love Yourself",
                    Length = 233
                  },
                  new Song
                  {
                    Title = "Company",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "No Pressure",
                    Length = 286
                  },
                  new Song
                  {
                    Title = "No Sense",
                    Length = 275
                  },
                  new Song
                  {
                    Title = "The Feeling",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "Life Is Worth Living",
                    Length = 234
                  },
                  new Song
                  {
                    Title = "Where Are Ü Now",
                    Length = 245
                  },
                  new Song
                  {
                    Title = "Children",
                    Length = 223
                  },
                  new Song
                  {
                    Title = "Purpose",
                    Length = 210
                  },
                  new Song
                  {
                    Title = "Been You",
                    Length = 199
                  },
                  new Song
                  {
                    Title = "Get Used to It",
                    Length = 238
                  },
                  new Song
                  {
                    Title = "We Are",
                    Length = 202
                  },
                  new Song
                  {
                    Title = "Trust",
                    Length = 203
                  },
                  new Song
                  {
                    Title = "All in It",
                    Length = 231
                  }
                }
              },
              new Album
              {
                Id = 55,
                Title = "Rainbow",
                Artist = "Kesha",
                ImageURL = "http://coverartarchive.org/release/f8647a3a-6df4-4be9-a4ac-2d04f1c1e333/18237647163-500.jpg",
                Rating = 4.5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Bastards",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Let ’em Talk",
                    Length = 185
                  },
                  new Song
                  {
                    Title = "Woman",
                    Length = 196
                  },
                  new Song
                  {
                    Title = "Hymn",
                    Length = 205
                  },
                  new Song
                  {
                    Title = "Praying",
                    Length = 230
                  },
                  new Song
                  {
                    Title = "Learn to Let Go",
                    Length = 217
                  },
                  new Song
                  {
                    Title = "Finding You",
                    Length = 173
                  },
                  new Song
                  {
                    Title = "Rainbow",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "Hunt You Down",
                    Length = 197
                  },
                  new Song
                  {
                    Title = "Boogie Feet",
                    Length = 173
                  },
                  new Song
                  {
                    Title = "Boots",
                    Length = 183
                  },
                  new Song
                  {
                    Title = "Old Flames (Can’t Hold a Candle to You)",
                    Length = 266
                  },
                  new Song
                  {
                    Title = "Godzilla",
                    Length = 129
                  },
                  new Song
                  {
                    Title = "Spaceship",
                    Length = 315
                  }
                }
              },
              new Album
              {
                Id = 56,
                Title = "Ray of Light",
                Artist = "Madonna",
                ImageURL = "http://coverartarchive.org/release/6e50e253-0601-42d5-8750-55c190f369b1/31882855698-500.jpg",
                Rating = 4.35d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Drowned World / Substitute for Love",
                    Length = 308
                  },
                  new Song
                  {
                    Title = "Swim",
                    Length = 300
                  },
                  new Song
                  {
                    Title = "Ray of Light",
                    Length = 320
                  },
                  new Song
                  {
                    Title = "Candy Perfume Girl",
                    Length = 276
                  },
                  new Song
                  {
                    Title = "Skin",
                    Length = 381
                  },
                  new Song
                  {
                    Title = "Nothing Really Matters",
                    Length = 266
                  },
                  new Song
                  {
                    Title = "Sky Fits Heaven",
                    Length = 287
                  },
                  new Song
                  {
                    Title = "Shanti / Ashtangi",
                    Length = 269
                  },
                  new Song
                  {
                    Title = "Frozen",
                    Length = 372
                  },
                  new Song
                  {
                    Title = "The Power of Good‐Bye",
                    Length = 252
                  },
                  new Song
                  {
                    Title = "To Have and Not to Hold",
                    Length = 322
                  },
                  new Song
                  {
                    Title = "Little Star",
                    Length = 318
                  },
                  new Song
                  {
                    Title = "Mer Girl",
                    Length = 331
                  }
                }
              },
              new Album
              {
                Id = 57,
                Title = "reputation",
                Artist = "Taylor Swift",
                ImageURL = "http://coverartarchive.org/release/c89bcb8c-224a-4b2f-811e-77bcfdd51422/32322354289-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "…Ready for It?",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "End Game",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "I Did Something Bad",
                    Length = 238
                  },
                  new Song
                  {
                    Title = "Don’t Blame Me",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "Delicate",
                    Length = 232
                  },
                  new Song
                  {
                    Title = "Look What You Made Me Do",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "So It Goes…",
                    Length = 227
                  },
                  new Song
                  {
                    Title = "Gorgeous",
                    Length = 209
                  },
                  new Song
                  {
                    Title = "Getaway Car",
                    Length = 233
                  },
                  new Song
                  {
                    Title = "King of My Heart",
                    Length = 214
                  },
                  new Song
                  {
                    Title = "Dancing With Our Hands Tied",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Dress",
                    Length = 230
                  },
                  new Song
                  {
                    Title = "This Is Why We Can’t Have Nice Things",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Call It What You Want",
                    Length = 203
                  },
                  new Song
                  {
                    Title = "New Year’s Day",
                    Length = 235
                  }
                }
              },
              new Album
              {
                Id = 58,
                Title = "Rock Steady",
                Artist = "No Doubt",
                ImageURL = "http://coverartarchive.org/release/9bbec3b9-6ef3-43af-adec-37b339811bc9/21311094013-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Intro",
                    Length = 27
                  },
                  new Song
                  {
                    Title = "Hella Good",
                    Length = 242
                  },
                  new Song
                  {
                    Title = "Hey Baby",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Making Out",
                    Length = 255
                  },
                  new Song
                  {
                    Title = "Underneath It All",
                    Length = 302
                  },
                  new Song
                  {
                    Title = "Detective",
                    Length = 173
                  },
                  new Song
                  {
                    Title = "Don’t Let Me Down",
                    Length = 249
                  },
                  new Song
                  {
                    Title = "Start the Fire",
                    Length = 251
                  },
                  new Song
                  {
                    Title = "Running",
                    Length = 241
                  },
                  new Song
                  {
                    Title = "In My Head",
                    Length = 206
                  },
                  new Song
                  {
                    Title = "Platinum Blonde Life",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Waiting Room",
                    Length = 268
                  },
                  new Song
                  {
                    Title = "Rock Steady",
                    Length = 324
                  }
                }
              },
              new Album
              {
                Id = 59,
                Title = "Rockferry",
                Artist = "Duffy",
                ImageURL = "http://coverartarchive.org/release/951ceafa-6f34-372b-8295-5f76a6a444b0/28526396896-500.jpg",
                Rating = 4.45d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Rockferry",
                    Length = 253
                  },
                  new Song
                  {
                    Title = "Warwick Avenue",
                    Length = 227
                  },
                  new Song
                  {
                    Title = "Serious",
                    Length = 251
                  },
                  new Song
                  {
                    Title = "Stepping Stone",
                    Length = 209
                  },
                  new Song
                  {
                    Title = "Syrup & Honey",
                    Length = 198
                  },
                  new Song
                  {
                    Title = "Hanging On Too Long",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "Mercy",
                    Length = 220
                  },
                  new Song
                  {
                    Title = "Delayed Devotion",
                    Length = 178
                  },
                  new Song
                  {
                    Title = "I’m Scared",
                    Length = 189
                  },
                  new Song
                  {
                    Title = "Distant Dreamer",
                    Length = 305
                  }
                }
              },
              new Album
              {
                Id = 60,
                Title = "Secrets",
                Artist = "Toni Braxton",
                ImageURL = "http://coverartarchive.org/release/59c49296-2d3d-403c-a9ea-32d3a34d516c/24796068478-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Come on Over Here",
                    Length = 217
                  },
                  new Song
                  {
                    Title = "You’re Makin Me High",
                    Length = 267
                  },
                  new Song
                  {
                    Title = "There’s No Me Without You",
                    Length = 259
                  },
                  new Song
                  {
                    Title = "Un‐Break My Heart",
                    Length = 269
                  },
                  new Song
                  {
                    Title = "Talking in His Sleep",
                    Length = 333
                  },
                  new Song
                  {
                    Title = "How Could an Angel Break My Heart",
                    Length = 260
                  },
                  new Song
                  {
                    Title = "Find Me a Man",
                    Length = 269
                  },
                  new Song
                  {
                    Title = "Let It Flow",
                    Length = 262
                  },
                  new Song
                  {
                    Title = "Why Should I Care",
                    Length = 265
                  },
                  new Song
                  {
                    Title = "I Don’t Want To",
                    Length = 257
                  },
                  new Song
                  {
                    Title = "I Love Me Some Him",
                    Length = 307
                  },
                  new Song
                  {
                    Title = "In the Late of Night / Toni’s Secrets",
                    Length = 329
                  }
                }
              },
              new Album
              {
                Id = 61,
                Title = "Sgt. Pepper’s Lonely Hearts Club Band",
                Artist = "The Beatles",
                ImageURL = "http://coverartarchive.org/release/82767576-902e-3173-a03e-d16721bfb988/32954776680-500.jpg",
                Rating = 4.55d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Sgt. Pepper’s Lonely Hearts Club Band",
                    Length = 122
                  },
                  new Song
                  {
                    Title = "With a Little Help From My Friends",
                    Length = 164
                  },
                  new Song
                  {
                    Title = "Lucy in the Sky With Diamonds",
                    Length = 209
                  },
                  new Song
                  {
                    Title = "Getting Better",
                    Length = 168
                  },
                  new Song
                  {
                    Title = "Fixing a Hole",
                    Length = 157
                  },
                  new Song
                  {
                    Title = "She’s Leaving Home",
                    Length = 215
                  },
                  new Song
                  {
                    Title = "Being for the Benefit of Mr. Kite!",
                    Length = 157
                  },
                  new Song
                  {
                    Title = "Within You Without You",
                    Length = 305
                  },
                  new Song
                  {
                    Title = "When I’m Sixty‐Four",
                    Length = 158
                  },
                  new Song
                  {
                    Title = "Lovely Rita",
                    Length = 162
                  },
                  new Song
                  {
                    Title = "Good Morning Good Morning",
                    Length = 161
                  },
                  new Song
                  {
                    Title = "Sgt. Pepper’s Lonely Hearts Club Band (reprise)",
                    Length = 79
                  },
                  new Song
                  {
                    Title = "A Day in the Life",
                    Length = 334
                  }
                }
              },
              new Album
              {
                Id = 62,
                Title = "Shawn Mendes",
                Artist = "Shawn Mendes",
                ImageURL = "http://coverartarchive.org/release/7f430c03-5aa3-46a9-acb3-3b999a8e0668/20468175708-500.jpg",
                Rating = 3.25d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "In My Blood",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Nervous",
                    Length = 164
                  },
                  new Song
                  {
                    Title = "Lost in Japan",
                    Length = 201
                  },
                  new Song
                  {
                    Title = "Where Were You in the Morning?",
                    Length = 201
                  },
                  new Song
                  {
                    Title = "Like to Be You",
                    Length = 159
                  },
                  new Song
                  {
                    Title = "Fallin’ All in You",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "Particular Taste",
                    Length = 176
                  },
                  new Song
                  {
                    Title = "Why",
                    Length = 238
                  },
                  new Song
                  {
                    Title = "Because I Had You",
                    Length = 142
                  },
                  new Song
                  {
                    Title = "Queen",
                    Length = 205
                  },
                  new Song
                  {
                    Title = "Youth",
                    Length = 190
                  },
                  new Song
                  {
                    Title = "Mutual",
                    Length = 148
                  },
                  new Song
                  {
                    Title = "Perfectly Wrong",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "When You’re Ready",
                    Length = 170
                  },
                  new Song
                  {
                    Title = "In My Blood (live session)",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "Where Were You in the Morning? (acoustic)",
                    Length = 179
                  }
                }
              },
              new Album
              {
                Id = 63,
                Title = "Songs From the West Coast",
                Artist = "Elton John",
                ImageURL = "http://coverartarchive.org/release/ad7fb687-a322-4dae-95b2-abe3e71e6192/7469892549-500.jpg",
                Rating = 3d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "The Emperor’s New Clothes",
                    Length = 268
                  },
                  new Song
                  {
                    Title = "Dark Diamond",
                    Length = 266
                  },
                  new Song
                  {
                    Title = "Look Ma, No Hands",
                    Length = 262
                  },
                  new Song
                  {
                    Title = "American Triangle",
                    Length = 290
                  },
                  new Song
                  {
                    Title = "Original Sin",
                    Length = 289
                  },
                  new Song
                  {
                    Title = "Birds",
                    Length = 232
                  },
                  new Song
                  {
                    Title = "I Want Love",
                    Length = 277
                  },
                  new Song
                  {
                    Title = "The Wasteland",
                    Length = 261
                  },
                  new Song
                  {
                    Title = "Ballad of the Boy in the Red Shoes",
                    Length = 292
                  },
                  new Song
                  {
                    Title = "Love Her Like Me",
                    Length = 238
                  },
                  new Song
                  {
                    Title = "Mansfield",
                    Length = 296
                  },
                  new Song
                  {
                    Title = "This Train Don’t Stop There Anymore",
                    Length = 280
                  }
                }
              },
              new Album
              {
                Id = 64,
                Title = "SOUR",
                Artist = "Olivia Rodrigo",
                ImageURL = "http://coverartarchive.org/release/92f78019-e5e6-4ce2-86ab-6057a8396f4a/29093344876-500.jpg",
                Rating = 3.5d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "brutal",
                    Length = 143
                  },
                  new Song
                  {
                    Title = "traitor",
                    Length = 229
                  },
                  new Song
                  {
                    Title = "drivers license",
                    Length = 242
                  },
                  new Song
                  {
                    Title = "1 step forward, 3 steps back",
                    Length = 163
                  },
                  new Song
                  {
                    Title = "deja vu",
                    Length = 215
                  },
                  new Song
                  {
                    Title = "good 4 u",
                    Length = 178
                  },
                  new Song
                  {
                    Title = "enough for you",
                    Length = 202
                  },
                  new Song
                  {
                    Title = "happier",
                    Length = 175
                  },
                  new Song
                  {
                    Title = "jealousy, jealousy",
                    Length = 173
                  },
                  new Song
                  {
                    Title = "favorite crime",
                    Length = 152
                  },
                  new Song
                  {
                    Title = "hope ur ok",
                    Length = 209
                  }
                }
              },
              new Album
              {
                Id = 65,
                Title = "Spirit",
                Artist = "Leona Lewis",
                ImageURL = "http://coverartarchive.org/release/f2ca6f03-00cc-45f5-9dec-8ae503507ad6/10925888213-500.jpg",
                Rating = 3d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Bleeding Love",
                    Length = 263
                  },
                  new Song
                  {
                    Title = "Better in Time",
                    Length = 234
                  },
                  new Song
                  {
                    Title = "I Will Be",
                    Length = 239
                  },
                  new Song
                  {
                    Title = "I’m You",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "Forgive Me",
                    Length = 221
                  },
                  new Song
                  {
                    Title = "Misses Glass",
                    Length = 221
                  },
                  new Song
                  {
                    Title = "Angel",
                    Length = 254
                  },
                  new Song
                  {
                    Title = "The First Time Ever I Saw Your Face",
                    Length = 266
                  },
                  new Song
                  {
                    Title = "Yesterday",
                    Length = 234
                  },
                  new Song
                  {
                    Title = "Whatever It Takes",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Take a Bow",
                    Length = 234
                  },
                  new Song
                  {
                    Title = "Footprints in the Sand",
                    Length = 248
                  },
                  new Song
                  {
                    Title = "Here I Am",
                    Length = 291
                  }
                }
              },
              new Album
              {
                Id = 66,
                Title = "Stripped",
                Artist = "Christina Aguilera",
                ImageURL = "http://coverartarchive.org/release/be8c6dcd-5a06-39b9-a423-5209ad0dd268/6980237754-500.jpg",
                Rating = 4.2d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Stripped (intro)",
                    Length = 99
                  },
                  new Song
                  {
                    Title = "Can’t Hold Us Down",
                    Length = 255
                  },
                  new Song
                  {
                    Title = "Walk Away",
                    Length = 347
                  },
                  new Song
                  {
                    Title = "Fighter",
                    Length = 247
                  },
                  new Song
                  {
                    Title = "Primer amor (interlude)",
                    Length = 53
                  },
                  new Song
                  {
                    Title = "Infatuation",
                    Length = 257
                  },
                  new Song
                  {
                    Title = "Loves Embrace (interlude)",
                    Length = 46
                  },
                  new Song
                  {
                    Title = "Loving Me 4 Me",
                    Length = 276
                  },
                  new Song
                  {
                    Title = "Impossible",
                    Length = 254
                  },
                  new Song
                  {
                    Title = "Underappreciated",
                    Length = 240
                  },
                  new Song
                  {
                    Title = "Beautiful",
                    Length = 240
                  },
                  new Song
                  {
                    Title = "Make Over",
                    Length = 252
                  },
                  new Song
                  {
                    Title = "Cruz",
                    Length = 229
                  },
                  new Song
                  {
                    Title = "Soar",
                    Length = 285
                  },
                  new Song
                  {
                    Title = "Get Mine, Get Yours",
                    Length = 224
                  },
                  new Song
                  {
                    Title = "Dirrty",
                    Length = 298
                  },
                  new Song
                  {
                    Title = "Stripped (part 2)",
                    Length = 46
                  },
                  new Song
                  {
                    Title = "The Voice Within",
                    Length = 304
                  },
                  new Song
                  {
                    Title = "I’m OK",
                    Length = 319
                  },
                  new Song
                  {
                    Title = "Keep On Singin’ My Song",
                    Length = 389
                  }
                }
              },
              new Album
              {
                Id = 67,
                Title = "Stronger",
                Artist = "Kelly Clarkson",
                ImageURL = "http://coverartarchive.org/release/ae13c2c9-0d87-4653-9cc3-379a9602714a/10152453902-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Mr. Know It All",
                    Length = 232
                  },
                  new Song
                  {
                    Title = "What Doesn’t Kill You (Stronger)",
                    Length = 221
                  },
                  new Song
                  {
                    Title = "Dark Side",
                    Length = 224
                  },
                  new Song
                  {
                    Title = "Honestly",
                    Length = 216
                  },
                  new Song
                  {
                    Title = "You Love Me",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "Einstein",
                    Length = 179
                  },
                  new Song
                  {
                    Title = "Standing in Front of You",
                    Length = 240
                  },
                  new Song
                  {
                    Title = "I Forgive You",
                    Length = 184
                  },
                  new Song
                  {
                    Title = "Hello",
                    Length = 179
                  },
                  new Song
                  {
                    Title = "The War Is Over",
                    Length = 238
                  },
                  new Song
                  {
                    Title = "Let Me Down",
                    Length = 204
                  },
                  new Song
                  {
                    Title = "You Can’t Win",
                    Length = 261
                  },
                  new Song
                  {
                    Title = "Breaking Your Own Heart",
                    Length = 229
                  },
                  new Song
                  {
                    Title = "Don’t You Wanna Stay",
                    Length = 259
                  },
                  new Song
                  {
                    Title = "Alone",
                    Length = 181
                  },
                  new Song
                  {
                    Title = "Don’t Be a Girl About It",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "The Sun Will Rise",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "Why Don’t You Try",
                    Length = 287
                  }
                }
              },
              new Album
              {
                Id = 68,
                Title = "Sweetener",
                Artist = "Ariana Grande",
                ImageURL = "http://coverartarchive.org/release/7e1a47a1-a9f5-4815-a7fb-c2183cd61ebc/20714144465-500.jpg",
                Rating = 4.2d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "raindrops (an angel cried)",
                    Length = 37
                  },
                  new Song
                  {
                    Title = "blazed",
                    Length = 196
                  },
                  new Song
                  {
                    Title = "the light is coming",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "R.E.M",
                    Length = 245
                  },
                  new Song
                  {
                    Title = "God is a woman",
                    Length = 197
                  },
                  new Song
                  {
                    Title = "sweetener",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "successful",
                    Length = 227
                  },
                  new Song
                  {
                    Title = "everytime",
                    Length = 172
                  },
                  new Song
                  {
                    Title = "breathin",
                    Length = 198
                  },
                  new Song
                  {
                    Title = "no tears left to cry",
                    Length = 205
                  },
                  new Song
                  {
                    Title = "borderline",
                    Length = 177
                  },
                  new Song
                  {
                    Title = "better off",
                    Length = 171
                  },
                  new Song
                  {
                    Title = "goodnight n go",
                    Length = 189
                  },
                  new Song
                  {
                    Title = "pete davidson",
                    Length = 73
                  },
                  new Song
                  {
                    Title = "get well soon",
                    Length = 322
                  }
                }
              },
              new Album
              {
                Id = 69,
                Title = "Taylor Swift",
                Artist = "Taylor Swift",
                ImageURL = "http://coverartarchive.org/release/f36b9775-89a8-4290-92c6-1f1988d2207b/11662988724-500.jpg",
                Rating = 4.4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Tim McGraw",
                    Length = 232
                  },
                  new Song
                  {
                    Title = "Picture to Burn",
                    Length = 175
                  },
                  new Song
                  {
                    Title = "Teardrops on My Guitar",
                    Length = 215
                  },
                  new Song
                  {
                    Title = "A Place in This World",
                    Length = 200
                  },
                  new Song
                  {
                    Title = "Cold as You",
                    Length = 239
                  },
                  new Song
                  {
                    Title = "The Outside",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Tied Together With a Smile",
                    Length = 248
                  },
                  new Song
                  {
                    Title = "Stay Beautiful",
                    Length = 236
                  },
                  new Song
                  {
                    Title = "Should’ve Said No",
                    Length = 242
                  },
                  new Song
                  {
                    Title = "Mary’s Song (Oh My My My)",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "Our Song",
                    Length = 202
                  }
                }
              },
              new Album
              {
                Id = 70,
                Title = "thank u, next",
                Artist = "Ariana Grande",
                ImageURL = "http://coverartarchive.org/release/f3c1f76d-d436-4402-8888-1fdcf9e6cba8/27301517082-500.jpg",
                Rating = 3.95d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "imagine",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "needy",
                    Length = 171
                  },
                  new Song
                  {
                    Title = "NASA",
                    Length = 182
                  },
                  new Song
                  {
                    Title = "bloodline",
                    Length = 216
                  },
                  new Song
                  {
                    Title = "fake smile",
                    Length = 208
                  },
                  new Song
                  {
                    Title = "bad idea",
                    Length = 267
                  },
                  new Song
                  {
                    Title = "make up",
                    Length = 140
                  },
                  new Song
                  {
                    Title = "ghostin",
                    Length = 271
                  },
                  new Song
                  {
                    Title = "in my head",
                    Length = 222
                  },
                  new Song
                  {
                    Title = "7 rings",
                    Length = 178
                  },
                  new Song
                  {
                    Title = "thank u, next",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "break up with your girlfriend, i’m bored",
                    Length = 190
                  }
                }
              },
              new Album
              {
                Id = 71,
                Title = "The E.N.D.",
                Artist = "The Black Eyed Peas",
                ImageURL = "http://coverartarchive.org/release/b0f2f06b-09b5-44ba-9e5d-73a25944d3f1/26274757363-500.jpg",
                Rating = 3.15d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Boom Boom Pow",
                    Length = 307
                  },
                  new Song
                  {
                    Title = "Ring‐a‐Ling",
                    Length = 272
                  },
                  new Song
                  {
                    Title = "Rock That Body",
                    Length = 268
                  },
                  new Song
                  {
                    Title = "Party All the Time",
                    Length = 283
                  },
                  new Song
                  {
                    Title = "Meet Me Halfway",
                    Length = 284
                  },
                  new Song
                  {
                    Title = "Out of My Head",
                    Length = 231
                  },
                  new Song
                  {
                    Title = "Imma Be",
                    Length = 256
                  },
                  new Song
                  {
                    Title = "Electric City",
                    Length = 248
                  },
                  new Song
                  {
                    Title = "I Gotta Feeling",
                    Length = 290
                  },
                  new Song
                  {
                    Title = "Showdown",
                    Length = 267
                  },
                  new Song
                  {
                    Title = "Alive",
                    Length = 302
                  },
                  new Song
                  {
                    Title = "Now Generation",
                    Length = 246
                  },
                  new Song
                  {
                    Title = "Missing You",
                    Length = 274
                  },
                  new Song
                  {
                    Title = "One Tribe",
                    Length = 280
                  },
                  new Song
                  {
                    Title = "Rockin to the Beat",
                    Length = 225
                  }
                }
              },
              new Album
              {
                Id = 72,
                Title = "The Fray",
                Artist = "The Fray",
                ImageURL = "http://coverartarchive.org/release/a60786a3-989b-4e70-971c-e9213173a630/24047012169-500.jpg",
                Rating = 3d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Syndicate",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "Absolute",
                    Length = 229
                  },
                  new Song
                  {
                    Title = "You Found Me",
                    Length = 243
                  },
                  new Song
                  {
                    Title = "Say When",
                    Length = 305
                  },
                  new Song
                  {
                    Title = "Never Say Never",
                    Length = 259
                  },
                  new Song
                  {
                    Title = "Where the Story Ends",
                    Length = 237
                  },
                  new Song
                  {
                    Title = "Enough for Now",
                    Length = 257
                  },
                  new Song
                  {
                    Title = "Ungodly Hour",
                    Length = 307
                  },
                  new Song
                  {
                    Title = "We Build Then We Break",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "Happiness",
                    Length = 324
                  }
                }
              },
              new Album
              {
                Id = 73,
                Title = "The River in Reverse",
                Artist = "Elvis Costello",
                ImageURL = "http://coverartarchive.org/release/d5bfef6e-e086-3711-aabd-8a754e76aa2b/31432825725-500.jpg",
                Rating = 4d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "On Your Way Down",
                    Length = 294
                  },
                  new Song
                  {
                    Title = "Nearer to You",
                    Length = 212
                  },
                  new Song
                  {
                    Title = "Tears, Tears and More Tears",
                    Length = 210
                  },
                  new Song
                  {
                    Title = "The Sharpest Thorn",
                    Length = 256
                  },
                  new Song
                  {
                    Title = "Who\'s Gonna Help Brother Get Further?",
                    Length = 304
                  },
                  new Song
                  {
                    Title = "The River in Reverse",
                    Length = 272
                  },
                  new Song
                  {
                    Title = "Freedom for the Stallion",
                    Length = 178
                  },
                  new Song
                  {
                    Title = "Broken Promise Land",
                    Length = 274
                  },
                  new Song
                  {
                    Title = "Ascension Day",
                    Length = 177
                  },
                  new Song
                  {
                    Title = "International Echo",
                    Length = 298
                  },
                  new Song
                  {
                    Title = "All These Things",
                    Length = 247
                  },
                  new Song
                  {
                    Title = "Wonder Woman",
                    Length = 188
                  },
                  new Song
                  {
                    Title = "Six-Fingered Man",
                    Length = 270
                  }
                }
              },
              new Album
              {
                Id = 74,
                Title = "This Fire",
                Artist = "Paula Cole",
                ImageURL = "http://coverartarchive.org/release/4c22ffc2-e79c-419b-960f-6a9f1381b360/21026190989-500.jpg",
                Rating = 3d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Tiger",
                    Length = 258
                  },
                  new Song
                  {
                    Title = "Where Have All the Cowboys Gone?",
                    Length = 266
                  },
                  new Song
                  {
                    Title = "Throwing Stones",
                    Length = 226
                  },
                  new Song
                  {
                    Title = "Carmen",
                    Length = 225
                  },
                  new Song
                  {
                    Title = "Mississippi",
                    Length = 306
                  },
                  new Song
                  {
                    Title = "Nietzsche’s Eyes",
                    Length = 332
                  },
                  new Song
                  {
                    Title = "Road to Dead",
                    Length = 221
                  },
                  new Song
                  {
                    Title = "ME",
                    Length = 302
                  },
                  new Song
                  {
                    Title = "Feelin’ Love",
                    Length = 337
                  },
                  new Song
                  {
                    Title = "hush, hush, hush.",
                    Length = 262
                  },
                  new Song
                  {
                    Title = "I Don’t Want to Wait",
                    Length = 319
                  }
                }
              },
              new Album
              {
                Id = 75,
                Title = "This Is Acting",
                Artist = "Sia",
                ImageURL = "http://coverartarchive.org/release/4b3c5af6-9ead-4ec8-b924-8f0cafb62b60/13380712878-500.jpg",
                Rating = 4.3d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Bird Set Free",
                    Length = 252
                  },
                  new Song
                  {
                    Title = "Alive",
                    Length = 263
                  },
                  new Song
                  {
                    Title = "One Million Bullets",
                    Length = 252
                  },
                  new Song
                  {
                    Title = "Move Your Body",
                    Length = 247
                  },
                  new Song
                  {
                    Title = "Unstoppable",
                    Length = 217
                  },
                  new Song
                  {
                    Title = "Cheap Thrills",
                    Length = 211
                  },
                  new Song
                  {
                    Title = "Reaper",
                    Length = 219
                  },
                  new Song
                  {
                    Title = "House on Fire",
                    Length = 241
                  },
                  new Song
                  {
                    Title = "Footprints",
                    Length = 193
                  },
                  new Song
                  {
                    Title = "Sweet Design",
                    Length = 145
                  },
                  new Song
                  {
                    Title = "Broken Glass",
                    Length = 264
                  },
                  new Song
                  {
                    Title = "Space Between",
                    Length = 288
                  }
                }
              },
              new Album
              {
                Id = 76,
                Title = "Unorthodox Jukebox",
                Artist = "Bruno Mars",
                ImageURL = "http://coverartarchive.org/release/22c33d6e-311e-448a-9835-3ff853e427ea/19028431573-500.jpg",
                Rating = 2.75d,
                Songs = new List<Song>
                {
                  new Song
                  {
                    Title = "Young Girls",
                    Length = 229
                  },
                  new Song
                  {
                    Title = "Locked Out of Heaven",
                    Length = 233
                  },
                  new Song
                  {
                    Title = "Gorilla",
                    Length = 244
                  },
                  new Song
                  {
                    Title = "Treasure",
                    Length = 178
                  },
                  new Song
                  {
                    Title = "Moonshine",
                    Length = 228
                  },
                  new Song
                  {
                    Title = "When I Was Your Man",
                    Length = 213
                  },
                  new Song
                  {
                    Title = "Natalie",
                    Length = 225
                  },
                  new Song
                  {
                    Title = "Show Me",
                    Length = 207
                  },
                  new Song
                  {
                    Title = "Money Make Her Smile",
                    Length = 203
                  },
                  new Song
                  {
                    Title = "If I Knew",
                    Length = 132
                  }
                }
              }
            };
        }
    }

}
