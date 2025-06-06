namespace vit_webapp.Models
{
    public class Movie
    {
        public string Название { get; set; }
        public string Жанры { get; set; }
        public string Описание { get; set; }
        public int Год_выпуска { get; set; }
        public string Постер { get; set; }
        public double Рейтинг { get; set; }
        public bool IsFavorite { get; set; }
    }
} 