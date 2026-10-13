# Conditional query results with case

Use `case(condition:result, ...)` in `$filter`, `$orderby`, or `$compute` to
choose a result based on Boolean conditions. For example, this query ranks
names containing `test` by its position and puts other names last:

```text
$compute=case(indexof(Name,'test') eq -1:10000,true:indexof(Name,'test')) as rank
&$select=Name&$orderby=rank
```

Conditions are evaluated from left to right. The first condition that is true
selects its result; later conditions and results are not evaluated. False and
null conditions fall through. A matching null result is returned as null. If
no condition matches, the result is null, including for numeric and other
value types. A final `true:result` pair supplies a fallback.

Supported results are compatible primitive or enumeration values. Numeric
results are promoted to a common type, and nullable and non-nullable values
of the same type can be combined. Cases can contain nested cases and other
supported functions.

`AllowedFunctions.Case` controls whether the function is allowed and is included
in `AllowedFunctions.AllFunctions`. Functions and restricted properties in
every branch are validated, including branches that are not selected at runtime.
