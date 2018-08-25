# Sitecore.Support.218625
[Azure Search] The content__ field should be marked only as searchable. Now indexing process can be interrupted with the following error message in the logs:
```
Field 'content__' contains a term that is too large to process. The max length for UTF-8 encoded terms is 32766 bytes. The most likely cause of this error is that filtering, sorting, and/or faceting are enabled on this field, which causes the entire field value to be indexed as a single term. Please avoid the use of these options for large fields.
```

## License  
This patch is licensed under the [Sitecore Corporation A/S License for GitHub](https://github.com/sitecoresupport/Sitecore.Support.218625/blob/master/LICENSE).  

## Download  
Downloads are available via [GitHub Releases](https://github.com/sitecoresupport/Sitecore.Support.218625/releases).  
