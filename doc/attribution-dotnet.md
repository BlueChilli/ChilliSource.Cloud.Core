# Attribution for ChilliSource .NET frameworks #

If you include code or libraries that are not yours, full disclosure is required, and the following rules must be followed:

* The license is [permissive](https://en.wikipedia.org/wiki/Permissive_free_software_licence)
* The license is left intact
* For third-party libraries, add an entry in the [third-party notices](https://github.com/BlueChilli/ChilliSource.Mobile/THIRD-PARTY-NOTICES) specifying the project, author(s), license type, and related urls. Example:

```
Rg.Plugins.Popup
________________________________________________________
Project: Rg.Plugins.Popup (https://github.com/rotorgames/Rg.Plugins.Popup)
Author:  Kirill Lyubimov (https://github.com/rotorgames)
License: MIT (https://github.com/rotorgames/Rg.Plugins.Popup/blob/master/LICENSE.md)
```

* For entire code files, add a comment block at the top of the file specifying the source, author(s), license type, and related urls, unless the file already includes all or some of this information. Example:

```
/*
Source: MvvmHelpers (https://github.com/jamesmontemagno/mvvm-helpers/blob/master/MvvmHelpers)
Author:  James Montemagno (https://github.com/jamesmontemagno)
License: Apache v2.0 (https://github.com/jamesmontemagno/mvvm-helpers/blob/master/LICENSE.md)
*/
```

* For code snippets taken from a blog or website such as StackOverflow, follow the attribution requirements of that site e.g. [StackOverflow attribution](https://stackoverflow.blog/2009/06/attribution-required/) and add a comment above the copied code specifying the website, page url, author name, and author url. Example:

```
/// <summary>
/// Loop through List<T> and grab each item
/// Source: StackOverflow(http://stackoverflow.com/questions/18863187/how-can-i-loop-through-a-listt-and-grab-each-item)
/// Author:  Simon Whitehead (http://stackoverflow.com/users/1517578/simon-whitehead)
/// </summary>
foreach (var money in myMoney)
{    
    Console.WriteLine("Amount is {0} and type is {1}", money.amount, money.type);
}
```

* For code that does not have a license, you have to get the author's permission in writing to use the code. One simple option is to log an issue on the repository to request that the author adds a license. Only once the license is available are you allowed to submit the code in your pull request.

