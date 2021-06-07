using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_LB_5
{
    public class GridBook 
    {
        public GridBook(Books obj)
        {
            Id = obj.Id;
            Title = obj.Title;
            Author = obj.Authors.AuthorName;
            Genre = obj.Genres.GenresName;
            Publisher = obj.Publishers.PublisherName;
            Series = obj.Series.SeriesName;
            Pages = obj.Pages;
            Year = obj.Year;
            Sale_price = obj.Sale_price;
            Coste_price = obj.Coste_price;
            Amount = obj.Amount;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public string Publisher { get; set; }
        public string Series { get; set; }
        public int Pages { get; set; }
        public int Year { get; set; }
        public decimal Sale_price { get; set; }
        public decimal Coste_price { get; set; }
        public int Amount { get; set; }
    }
}
