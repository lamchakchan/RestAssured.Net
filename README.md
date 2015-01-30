# RestAssured.Net
A simple testing suite for REST based interfaces

## TODO
- Add Load testing function.  Test for completion and average RTT.
- Add hooks for testing against Http Meta such as content type, content size and so forth
- Add features for processing Xml and possibly other formats.

## Download
### Nuget
[Nuget RestAssured.Net Info](https://www.nuget.org/packages/RestAssured/)
```Text
PM> Install-Package RestAssured
```

## Example
### Response
```JSON
{
  "ip":"2605:6000:f705:ab00:78e3:1959:78d4:bd76",
  "about":"/about",
  "Pro!":"http://getjsonip.com"
}
```
### Test
```C#
//Create a new test suite
new RestAssured()
  .Given()
    //Optional, set the name of this suite
    .Name("JsonIP Test Suite")
    //Optional, set the header parameters.  
    //Defaults will be set to application/json if none is given
    .Header("Content-Type", "application/json")
    .Header("Accept-Encoding", "gzip,deflate")
  .When()
    //url
    .Get("http://jsonip.com")
  .Then()
    //Give the name of the test and a lambda expression to test with
    //The lambda expression keys off of 'x' which represents the json blob as a dynamic.
    .Test("test a", x => x.about != null)
    //Throw an AssertException if the test case is false.
    .Assert("test a");
```

## Structure
The call chains are structured around 4 main parts.  
  1. Newing up RestAssured gives you a chainable object to work with
  2. All commands under Given() are setup procedures
    - Setup Headers
    - Setup Parameters
    - Setup Host and Uri Information
  3. All commands under When() are used for configuration the call environment
    - eg: GET vs POST
  4. All commands under Then() are used for setting up test rules and assertions

## Commands

### Given
```C#
  .Given()
    //Set up the name for a test suite
    .Name("string")
    //Set up Http Header
    //eg: "Accept-Encoding", "gzip, deflate"
    .Header("string", "string")
    //Set up Parameters
    //There is magic that will automatically create querystrings or header values calculated from the Http verb
    .Param("string", "string")
    //Allows for a body of content.  Only used for POSt and PUT verb
    .Body("string")
    //Set the host of the target server
    //Useful when you want to reuse a test suite between multiple Uris
    .Host("string")
    //Set the uri for the target endpoint
    .Uri("string")
    //Debug the settings
    .Debug()
```

### When
```C#
  .When()
    //Add a load test to the mix.  Set the # of threads and # of secs to run
    .Load(int, int)
    //Use the GET verb with a url.  This will override the above section.
    .Get("string")
    //Use the Get verb without and rely on url settings from Given() section
    .Get()
    //Use the POSt verb with a url.  This will override the above section.
    .Post("string")
    //Use the Post verb without and rely on the url settings from Given() section.
    .Post()
    //Debug the settings
    .Debug()
```

### Then
```C#
  .Then()
    //Write a test to make an assertion with
    //eg: "test A", x => x.id != null
    .Test("string", Func<dynamic, bool>)
    //Assert your test.  Failed test will throw an AssertException
    .Assert("string")
    //Apply a v3 or v4 json schema to the response.  Corrupted schemas will throw an ArgumentException
    .Schema("string")
    //Assert your schema against the response.  Failed test will throw an AssertException
    .AssertSchema()
    //Assert all your test.  Failed test will throw an AssertException
    .AssertAll()
    //Output all of the test results
    .WriteAssertions()
    //Debug the response
    .Debug()
```

## JSON Schema Support
ResAssured leverages Newtonsoft.Json for its JSON parsing and JSON schema validation support.  
Newtonsoft.Json on supports draft 3 and draft 4 of the JSON scheam specification, so we can too!

Utilize these tools to validate your JSON schema

[v3 Schema Validator](http://jsonschema.net/previous/)

[v4 Schema Validator](http://json-schema-validator.herokuapp.com/)

## More Examples

### Breaking up a call chain
```C#
//Create a new test suite
var endpoint1 = new RestAssured()
  .Given()
    //Optional, set the name of this suite
    .Name("JsonIP Test Suite")
    //Optional, set the header parameters.  
    //Defaults will be set to application/json if none is given
    .Header("Content-Type", "application/json")
    .Header("Accept-Encoding", "gzip,deflate");
    .Host("jsonip.com")
    .Uri("/endpoint1");

//Make a copy of the settings from above, but adjust the endpoint.
var endpoint2 = endpoint1.Clone().Uri("/endpoint2");

//Do a GET action with the first endpoint configuration
endpoint1.When().Get().Then().Test("test 1", x => x.id != null).Assert("test 1");

//Do a POSt action with the second endpoint configuration
endpoint2.When().Post().Then().Test("test 1", x => x.id != null).Assert("test 1");
```

### Load Test
```C#
new RestAssured()
    .Given()
        .Name("JsonIP single thread")
    .When()
        //Configure a load test with
        //6 threads that runs for 30 seconds
        .Load(6, 30)
        //Using this address
        .Get("http://yourremote.com")
    .Then()
        //Print out the results
        .Debug();
```
