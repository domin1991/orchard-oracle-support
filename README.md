Orchard 1.8.1 with Oracle 11g support

To run Orchard with Oracle database you have to copy Oracle client dlls to Orchard.Web\bin directory.
Then you have to add application path to path variable. You can modify constructor in Orchard.Web\Global.asax.cs

``` csharp
    public MvcApplication() {
        // add application path to path variable for current process to be able to use DB drivers stored here (eg. Oracle)
        System.Environment.SetEnvironmentVariable("path",
            System.Environment.GetEnvironmentVariable("path") + ";" +
            System.AppDomain.CurrentDomain.RelativeSearchPath);
    }
```


Oracle limitations:
- name of [table, column, index, constraint] is limited to 30 characters
- for text-value column there is four different data types(with different permitted length): NVARCHAR2(2000), VARCHAR2(4000), CLOB, NCLOB
- column names like 'Start', 'Number' are Oracle specific keywords and have to be quoted in sql queries


