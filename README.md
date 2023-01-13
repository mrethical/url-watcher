# url-watcher

Watch url for any DOM/content changes.

### Building the source
```
dotnet publish --configuration Release
```

### Usage
```
url-watcher [url] [options]
```

Options:
- `-i|--interval` - Ping interval (milliseconds).
- `-x|--xpath` - XPath selector to track only a section of the page.
