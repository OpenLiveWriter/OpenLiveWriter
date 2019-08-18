# Hacking - Static Site

The Static Site Generator support is split into various classes, as indicated by the class diagram below.

![](images/StaticSiteClassDiagram.png)

Please note that this diagram only describes classes created as part of the Static Site Generator implementation, and only classes in the `OpenLiveWriter.BlogClient.Clients.StaticSite` namespace. Other classes are implemented, such as wizard pages, which have been ommited from the above diagram.

## Class roles and descriptions

|Name|Role and description|Dependency summary|
|---|---|---|
|StaticSiteClient|Serves as the primary interface between Open Live Writer and the Static Site Generator support. Implements Blog functions such as CRUD on posts and pages.|<ul><li>Implements IBlogClient</li><li>Inherits BlogClientBase</li><li>Creates a private `StaticSiteConfig` and loads its contents from the provided `IBlogClientCredentialsAccessor` on initiation.</li></ul>|
|StaticSiteConfig|Defines and handles the storage and processing of configuration settings for the Static Site Generator support.|<ul><li>Instantiated in `StaticSiteClient`</li><li>Used-by-reference in `StaticSiteConfigDetector`.</li><li>Instantiates a `StaticSiteConfigFrontMatterKeys`, to be loaded from stored config values.</li></ul>|
|StaticSiteItem|Abstract class which represents a published or yet-to-be-published generic item to `StaticSiteClient`. Contains methods related to loading and saving, as well as generic methods which are based upon through sub-classes.|<ul><li>Contains a `BlogPost` (from `OpenLiveWriter.Extensibility.BlogClient`)</li><li>Generates a `StaticSiteItemFrontMatter` on attribute request</li><li>Abstract class, never instantiated.</li></ul>|
|StaticSitePost|Sub-class of `StaticSiteItem` representing a Post item.|Sub-classes `StaticSiteItem`|
|StaticSitePage|Sub-class of `StaticSiteItem` representing a Page item.|Sub-classes `StaticSiteItem`|
|StaticSiteItemFrontMatter|Defines and stores all possible static item front matter keys. Implements loading and saving from YAML, as well as loading and saving from a `OpenLiveWriter.Extensibility.BlogClient.BlogPost` instance.|<ul><li>Instantiated by `StaticSiteItem` on request.</li><li>Retrieves and stores a `StaticSiteConfigFrontMatterKeys` from `StaticSiteConfig` on construction.</li></li></ul>|
|StaticSiteConfigFrontMatterKeys|A subset of the static site config, `StaticSiteConfigFrontMatterKeys` contains the key names for each of the supported front-matter attributes. Used to support different static site generators.|<ul><li>Instantiated by StaticSiteConfig.</li><li>Used-by-reference in StaticSiteItemFrontMatter</li></ul>|