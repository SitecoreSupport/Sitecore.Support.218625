namespace Sitecore.Support.ContentSearch.Azure.Schema
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Linq;

  using Sitecore.ContentSearch;
  using Sitecore.ContentSearch.Azure;
  using Sitecore.ContentSearch.Azure.Config;
  using Sitecore.ContentSearch.Azure.FieldMaps;
  using Sitecore.ContentSearch.Azure.Models;
  using Sitecore.ContentSearch.Azure.Schema;
  using Sitecore.Diagnostics;

  internal class CloudSearchIndexSchemaBuilder : ICloudSearchIndexSchemaBuilder2
  {
    private readonly IDictionary<string, IndexedField> fields;

    private ICloudSearchTypeMapper typeMap;

    private ICloudFieldMap fieldMap;

    [NotNull]
    private static readonly Type cloudSearchIndexSchemaType;

    static CloudSearchIndexSchemaBuilder()
    {
      var csAzureAssembly = typeof(Sitecore.ContentSearch.Azure.CloudSearchProviderIndex).Assembly;
      var cloudSearchIndexSchemaType = csAzureAssembly.GetType("Sitecore.ContentSearch.Azure.Schema.CloudSearchIndexSchema");

      Assert.IsNotNull(cloudSearchIndexSchemaType, "cloudSearchIndexSchemaType != null");
    }

    public CloudSearchIndexSchemaBuilder()
    {
      this.fields = new ConcurrentDictionary<string, IndexedField>();
    }

    public void AddField(string fieldName, object fieldValue)
    {
      var field = this.BuildField(fieldName, fieldValue);

      if (field != null)
      {
        this.AddFields(field);
      }
    }

    public void AddFields(params IndexedField[] fileds)
    {
      lock (this)
      {
        foreach (var field in fileds)
        {
          if (this.fields.ContainsKey(field.Name))
          {
            if (!this.IsCompatibleType(field.Type, this.fields[field.Name].Type))
            {
              throw new ApplicationException(
                  $"Conflict between type of incomming field '{field.Type}' and field in schema '{this.fields[field.Name].Type}'");
            }
          }
          else
          {
            if (!this.fields.ContainsKey(field.Name))
            {
              this.fields.Add(field.Name, field);
            }
          }
        }
      }
    }

    protected virtual bool IsCompatibleType(string valueType, string storageType)
    {
      if (valueType == "Edm.String" && storageType == "Collection(Edm.String)")
      {
        return true;
      }

      return storageType == valueType;
    }

    public ICloudSearchIndexSchema GetSchema()
    {
      // return new CloudSearchIndexSchema(this.fields.Values.ToList());
      return this.CreateCloudSearchIndexSchema(this.fields.Values.ToList());
    }

    private ICloudSearchIndexSchema CreateCloudSearchIndexSchema(IEnumerable<IndexedField> fields)
    {
      return Activator.CreateInstance(cloudSearchIndexSchemaType, new[] { fields }) as ICloudSearchIndexSchema;
    }

    public void Initialize(ProviderIndexConfiguration indexConfiguration)
    {
      var configuration = indexConfiguration as CloudIndexConfiguration;
      if (configuration == null)
      {
        throw new NotSupportedException($"Only {typeof(CloudIndexConfiguration).Name} is supported.");
      }

      this.fieldMap = (ICloudFieldMap)configuration.FieldMap;
      this.typeMap = configuration.CloudTypeMapper;
    }

    private IndexedField BuildField(string fieldName, object fieldValue)
    {
      var config = this.fieldMap.GetCloudFieldConfigurationByCloudFieldName(fieldName);

      if (config?.Ignore == true)
      {
        return null;
      }

      string cloudType;

      if (config?.Type != null)
      {
        cloudType = this.typeMap.GetEdmTypeName(config.Type);
      }
      else
      {
        if (fieldValue == null)
        {
          return null;
        }

        cloudType = this.typeMap.GetEdmTypeName(fieldValue.GetType());
      }
      var isKey = fieldName.Equals(CloudSearchConfig.VirtualFields.CloudUniqueId);
      var isSearchable = cloudType == "Edm.String" || cloudType == "Collection(Edm.String)";
      // TODO replace hardcode with configuration setting.
      var isRetrievable = fieldName != "content__";
      return new IndexedField(fieldName, cloudType, isKey, isSearchable, isRetrievable) { Analyzer = config?.CloudAnalyzer };
    }

    public void Reset()
    {
      this.fields.Clear();
    }
  }
}