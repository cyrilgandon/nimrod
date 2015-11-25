# Nimrod

Nimrod is an ASP.NET MVC to TypeScript Converter.

That means that it will take all your exising ASP.NET MVC application, and generate TypeScript models and services correponding to your C# code.

Generally, you have to write the model code two times, one time in the backend langage (Java, Ruby, PHP, C#), and one time in the frontend langage (JavaScript).

Unless your backend is node.js. This implies a lot of boilerplate code.

This library allow you to skip the step to rewrite your frontend code by generating all the TypeScript code for you, so you can use the power of strongly type langage like TypeScript.

# Example

C# code
```
public class Movie
{
    public string Name { get; }
    public double Rating { get; }
    public List<string> Actors { get; }
}
public class MovieController : Controller
{
    [HttpGet]
    public JsonNetResult<Movie> Movie(int id)
    {
        throw new NotImplementedException();
    }
}
```
Generated TypeScript code
```
module Nimrod.Test.ModelExamples {
    export interface IMovie {
        Name: string;
        Rating: number;
        Actors: string[];
    }
}
module Nimrod.Test.ModelExamples {
    export interface IMovieService {
        Movie(restApi: Nimrod.IRestApi, id: number, config?: Nimrod.IRequestShortcutConfig): Nimrod.IPromise<Nimrod.Test.ModelExamples.IMovie>;
    }
    export class MovieService implements IMovieService {
        public Movie(restApi: Nimrod.IRestApi, id: number, config?: Nimrod.IRequestShortcutConfig): Nimrod.IPromise<Nimrod.Test.ModelExamples.IMovie> {
            (config || (config = {})).params = {
                id: id,
            };
            return restApi.Get<Nimrod.Test.ModelExamples.IMovie>('/Movie/Movie', config);
        }
    }
    service('serverApi.MovieService', MovieService);
}

```
Interfaces `IRequestShortcutConfig` and `IPromise` should be added accordingly to your javascript framework. It could be Angular or React or whatever, here is an example that works for Angular:

```
module Nimrod {
    export interface IRequestShortcutConfig extends ng.IRequestShortcutConfig {
    }
	export interface IPromise<T> extends ng.IPromise<T> {
    }
}
```
The `restApi` parameter is a wrapper you must write that will wrap the logic of the ajax request. The wrapper have to implement the `Nimrod.IRestApi` interface which is composed of the four methods Get, Post, Put and Delete.

Example with Angular and the [$http](https://docs.angularjs.org/api/ng/service/$http) angular service:

```
class RestApi implements Nimrod.IRestApi {
    static $inject: string[] = ['$http'];
    constructor(private $http: ng.IHttpService) {
    }

    public Get<T>(url: string, config?: ng.IRequestShortcutConfig) {
        return this.$http.get<T>(url, config)
    }

    public Post<T>(url: string, data: any, config?: ng.IRequestShortcutConfig) {
        return this.$http.post<T>(url, data, config);
    }

    public Put<T>(url: string, data: any, config?: ng.IRequestShortcutConfig) {
        return this.$http.put<T>(url, data, config);
    }

    public Delete<T>(url: string, config?: ng.IRequestShortcutConfig) {
        return this.$http.delete<T>(url, config);
    }
}
```

# How does it works?

When you launch Nimrod, the following steps are going to happen:

 - Read all the assembly to search for class which inherits from [Web.Mvc.Controller](https://msdn.microsoft.com/library/system.web.mvc.controller)
 - From thoses classes, search public method which have one of the following attribute [HttpGet], [HttpPost], [HttpPut], [HttpDelete]
 - Search all classes referenced by the generic return of those methodes (see below), and their arguments
 - Generate TypeScript files for all those classes

# Usage

### Command line

Use the Nimrod.Console utilities to generate files
```
Nimrod.Console.exe -m typescript -o .\\src\\ServerApi.Generated --files=..\\assembly1.dll:..\\assembly2.dll',
```
###  Options

|Name|Alias|Description|
|:----|:----|:-----|
|--module|-m|Module mode, valid values are `typescript` for [typescript] modules style and `require` for [requirejs] modules|
|--output|-o|Directory where files will be generated|
|--files|-f|Assembly files to read, separated by a colon. Example : --files=bin\\Assembly1.dll:bin\\Assembly2.dll|
|--verbose|-v|Prints all messages to standard output|
|--help|-h|Prints all messages to standard output|

---
# Grunt
A task named grunt-nimrod have been created to easily use the grunt task manager to generate the typescrips files.

### Getting Started

If you haven't used [Grunt](http://gruntjs.com/) before, be sure to check out the [Getting Started](http://gruntjs.com/getting-started) guide, as it explains how to create a [Gruntfile](http://gruntjs.com/sample-gruntfile) as well as install and use Grunt plugins. Once you're familiar with that process, you may install this plugin with this command:

```shell
npm install grunt-nimrod --save-dev
```

Once the plugin has been installed, it may be enabled inside your Gruntfile with this line of JavaScript:

```js
grunt.loadNpmTasks('grunt-nimrod');
```

### Example
```
module.exports = function(grunt) {
  grunt.initConfig({
    nimrod: {
      default_options: {
        options: {
			module: 'typescript',
			output: 'C:\\temp\\nimrod-test-generated',
			files : ['C:\\path\\to\\assembly1\\assembly1.dll'
				    ,'C:\\path\\to\\assembly2\\assembly2.dll']
        }
      }
    }
  });
  grunt.loadNpmTasks('grunt-nimrod');
}
```

---
# Questions & Answers

**Q: Why the generator didn't write the class JSONNetResult?**

On the C# side, the return type of a controller method is always wrapped inside a `Json` something stuff. As we need to detect the real type of the return, we oblige the user of our library to wrap the return type into a generic. And so we don't generate the model itself, but the type inside of it. So if the return type is `Foo<Bar>`, we are going to generate only `Bar`, not `Foo`.

**Q: I only need those models generators, not all the fancy service controller whatever stuff, can you do it?**

We plan to deliver a version of the converter only for POCOs, but pull requests are welcomed

**Q: Where did the name 'Nimrod' come from?**

Like every other convertor/translator in the world, we would ilke to name the library 'Babel'. That seems not a good idea on a [search].
So we look at who built Babel, and it was a dude called [Nimrod], that's it!


# Todos

 - Refactoring
 - Docs
 - Limitations on generics, specifically return embed generics. IE : a method returning a `Json<List<Tuple<int, string>>>` is not going to work

   [Nimrod]: <https://en.wikipedia.org/wiki/Nimrod>
   [search]: <https://www.npmjs.com/search?q=babel>
   [typescript]: <http://www.johnpapa.net/typescriptpost4>
   [requirejs]: <http://requirejs.org/>
   [HttpGet]: <https://msdn.microsoft.com/library/system.web.mvc.httpgetattribute.aspx>
   [HttpPost]: <https://msdn.microsoft.com/library/system.web.mvc.httppostattribute.aspx>
   [HttpPut]: <https://msdn.microsoft.com/library/system.web.mvc.httpputattribute.aspx>
   [HttpDelete]: <https://msdn.microsoft.com/library/system.web.mvc.httpdeleteattribute.aspx>


