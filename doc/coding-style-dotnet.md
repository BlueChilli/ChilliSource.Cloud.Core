# Codin style for ChilliSource .NET frameworks #

## Naming Guidelines ##

### General ###

* use PascalCasing for all types and methods, except private fields
* use camelCasing for parameters and private fields

* do not use Hungarian notation
* choose easily readable identifier names
* prefer descriptive naming that makes the purpose of the identifier clear
* do not use abbreviations, except if they are widely accepted, e.g. Id, Url, IO, Http etc.
* avoid single letter variable names except in short pieces of code like lambda identifiers and loop variables
* avoid identifiers that conflict with C# keywords and .NET types

### Types ###

* use noun phrases to name types
* consider ending the name of a derived class with the name of the base class, e.g. `ArgumentOutOfRangeException`

### Interfaces ###

* prefix interfaces with an 'I'
* prefer using adjective phrases to name interfaces

### Properties ###

* use noun phrases to name properties
* name boolean properties with an affirmative phrase, e.g. `CanSeek` instead of `CannotSeek`
* prefix boolean properties with 'Is', 'Has', 'Can'
* consider giving a property the same name as its type, e.g. `public Color Color {get;set;}`
* prefer adjective + noun phrasing e.g. CreatedDate not DateCreated
* don't use 'On' or 'At' in property names e.g. CreatedDate not CreatedOn

### Fields ###

* use camel casing for private fields
* use underscore to prefix private fields e.g. _firstName;
* use PascalCasing for constants

### Methods ###

* use verb phrases to name methods, e.g. GetStream() instead of Stream()
* avoid parameter information in the method name e.g. GetUserByName()
* use camelCasing for parameter names
* use descriptive parameter names
* consider using names based on a parmeter's meaning rather than its type

### Events ###

* use verb phrases to name events
* use the present and past tense to indicate when the events occurred, e.g. `Closed` and `Closing`
* add the 'EventHandler' suffix to the name of event handler delegates e.g. 
`public delegate void ClickedEventHandler(object sender, ClickedEventArgs e);`
* add the 'EventArgs' suffix to the name of event argument classes

## File Organization ##

* specify namespace imports at the top of the file, outside of `namespace` declarations
* strongly prefer placing each public type in a separate file, however multiple internal classes are allowed per file
* name the file the same as the public type it contains
* in general prefer grouping members into sections in the following order: Fields, Constructors, Properties, Events, Methods, Nested Types. Backing fields can be placed with their corresponding properties.
* in xaml files place attributes on a new line and align them underneath each other
* for non-code files keep new code and changes consistent with the style in the existing files
* if a file happens to differ in style from these guidelines the existing style in that file takes precedence

### Spacing ###

* use four spaces of indentation instead of tabs
* avoid more than one empty line at any time
* avoid spurious free spaces, e.g. avoid `if (someVar == 0)...`, where the dots mark the spurious free spaces
* for properties and methods spanning more than one line (including attributes and comments), add an empty line to separate it from the rest of the code

## Type Design Guidelines ##

### General Type Design ###

* always specify the visibility of members, even if it's the default e.g. `private string _foo` not `string _foo` 
* specify the visibility modifier first, e.g. `public abstract` not `abstract public`
* always make nested types private
* use ```nameof(...)``` instead of ```"..."``` whenever possible and relevant
* use [Allman style](http://en.wikipedia.org/wiki/Indent_style#Allman_style) braces, where each brace begins on a new line, even for single line statements. This increases code readability and maintainability, and reduces the likelihood of bugs.
* for internal classes that need to be visible to unit tests, use the `InternalVisibleTo` attribute 
* when using the `Obsolete` attribute, mention the replacement method to be used

### Fields ###

* set the visibility of fields to private
* use properties to expose fields to outside the encompassing type

### Properties ###

* use properties as 'smart fields' with only simple operations that are not CPU or I/O intensive - use methods instead
* do not create set-only properties - use methods instead
* if a getter can throw an exception, consider redesigning the property to be a method

### Exceptions ###

* code flow / logic should not rely on exceptions
* use specific exception subclasses to indicate what the problem is instead of the base Exception class
* prefer allowing exceptions to bubble up and handle them globally
* if handling specific exception make sure application can recover gracefully from exception
* when throwing exceptions use `throw;` instead of `throw ex;` to maintain stack trace

### Enums ###

* use an enum to strongly type parameters, properties, and return values that represent sets of values
* favor using an enum instead of static constants
* name simple enums with singular noun phrases and flag enums with plural noun phrases

#### New Versions of Existing API Methods ####

* use a name similar to the old API when creating new versions of an existing API
* prefer adding a suffix rather than a prefix to indicate a new version of an existing API
