
namespace Sitecore.Support
{
  public class ExtendedIndexedField : Sitecore.ContentSearch.Azure.Models.IndexedField
  {
    public ExtendedIndexedField(string name, string type, bool key, bool searchable) : base(name, type, key, searchable)
    {
    }

    public ExtendedIndexedField(string name, string type, bool key, bool searchable, bool retrievable, bool sortable, bool facetable, bool filterable)
      : base(name, type, key, searchable, retrievable)
    {
      this.Sortable = sortable;
      this.Filterable = filterable;
      this.Facetable = facetable;
    }

    public bool Sortable { get; }

    public bool Facetable { get; }

    public bool Filterable { get; }
  }
}