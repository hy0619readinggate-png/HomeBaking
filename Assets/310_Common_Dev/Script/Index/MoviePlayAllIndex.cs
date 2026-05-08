using System.Linq;

namespace DoDoEng.Common
{
    public class MoviePlayAllIndex : IndexBase
    {
        // Properties
        public int Count => movies.Length;
        public MovieSingleIndex[] Movies => movies;

        // Methods : ctor.
        public MoviePlayAllIndex(string[] ebCodes)
        {
            movies = ebCodes.Select(idx => new MovieSingleIndex(idx))
                              .ToArray();
        }

        // Methods 
        public MovieSingleIndex GetMovieIndex(int i) => movies[i];



        // Fields
        private MovieSingleIndex[] movies = null;

        // Overrides
        protected override string onGetSceneName()
        {
            return $"Movie_PlayAll";
        }
        protected override string onGetAddressablePath()
        {
            return string.Empty;
        }
        protected override string onGetThumbnailPath()
        {
            return string.Empty;
        }
        protected override string onGetDownloadDataPath()
        {
            return string.Empty;
        }
        public override string ToString()
        {
            return $"<b><color=white>MoviePlayAllIndex (Count:{Count})</color></b>";
        }
    }
}