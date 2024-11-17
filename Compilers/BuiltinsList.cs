namespace Epsilon;
public static class BuiltinsList {
    public static List<ExternalFunction> Builtins = [
        new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "len")
                ], new SlotPatternProcessor([0])
            ), [
                new("array", Type_.Any().ArrayOf())
            ], "builtin1", new Type_("W", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "capacity")
                ], new SlotPatternProcessor([0])
            ), [
                new("array", Type_.Any().ArrayOf())
            ], "builtin2", new Type_("W", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "append"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("array", Type_.Any().ArrayOf()),
                new("value", Type_.Any())
            ], "builtin3", (List<Type_> types_) => {
                Type_ value = types_[1];
                Type_ generic = types_[0].GetGeneric(0);
                if (!value.IsConvertibleTo(generic))
                    throw new FunctionCallTypes_Exception($"Cannot append type {value} to an array of type {types_[0]}", 1);
                return null;
            }, FunctionSource.Builtin, doesReturnVoid: true
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "require_capacity"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("array", Type_.Any().ArrayOf()),
                new("value", new Type_("W", 64))
            ], "builtin4", FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "shrink_mem"),
                ], new SlotPatternProcessor([0])
            ), [
                new("array", Type_.Any().ArrayOf()),
            ], "builtin5", FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "pop"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("array", Type_.Any().ArrayOf()),
                new("index", new Type_("W", 64)),
            ], "builtin6", (List<Type_> types_) => {
                return types_[0].GetGeneric(0);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "insert"),
                    new FuncArgPatternSegment(),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3, 4])
            ), [
                new("array", Type_.Any().ArrayOf()),
                new("index", new Type_("W", 64)),
                new("value", Type_.Any()),
            ], "builtin7", (List<Type_> types_) => {
                Type_ value = types_[2];
                Type_ generic = types_[0].GetGeneric(0);
                if (!value.IsConvertibleTo(generic))
                    throw new FunctionCallTypes_Exception($"Cannot insert type {value} into an array of type {types_[0]}", 2);
                return null;
            }, FunctionSource.Builtin, doesReturnVoid: true
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "clone"),
                ], new SlotPatternProcessor([0])
            ), [
                new("array", Type_.Any().ArrayOf()),
            ], "builtin8", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "extend"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("array", Type_.Any().ArrayOf()),
                new("array2", Type_.Any().ArrayOf()),
            ], "builtin9", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot extend array of type {types_[0]} with an array of type {types_[1]}", 1);
                return null;
            }, FunctionSource.Builtin, doesReturnVoid: true
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "concat"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("array1", Type_.Any().ArrayOf()),
                new("array2", Type_.Any().ArrayOf()),
            ], "builtin10", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception(
                        $"Cannot concat array of type {types_[0]} with an array of type {types_[1]}", 1
                    );
                return types_[0];
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "unsafe_idx"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("array", Type_.Any().ArrayOf()),
                new("index", new Type_("W", 64)),
            ], "builtin11", (List<Type_> types_) => {
                return types_[0].GetGeneric(0);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "unsafe"),
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("~"),
                    new TextPatternSegment("/"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([1, 4])
            ), [
                new("a", new Type_("Z")),
                new("b", new Type_("Z")),
            ], "builtin12", (List<Type_> types_) => {
                Type_ common = Type_.CommonOrNull(types_[0], types_[1]);
                if (common == null) {
                    throw new FunctionCallTypes_Exception(
                        "Incompatible types for division", 0
                    );
                }
                return common;
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "unsafe_idx"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("="),
                    new TypePatternSegment(typeof(RawSquareGroup))
                ], new SlotPatternProcessor([0, 3, 5])
            ), [
                new("array", Type_.Any().ArrayOf()),
                new("index", new Type_("W", 64)),
                new("value", Type_.Any())
            ], "builtin13", (List<Type_> types_) => {
                if (!types_[2].IsConvertibleTo(types_[0].GetGeneric(0)))
                    throw new FunctionCallTypes_Exception($"Cannot assign type {types_[2]} into an array of type {types_[0]}", 2);
                return null;
            }, FunctionSource.Builtin, doesReturnVoid: true
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new TextPatternSegment("|"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("|")
                ], new SlotPatternProcessor([1])
            ), [
                new("value", new Type_("Z"))
            ], "builtin14", (List<Type_> types_) => new Type_(
                "W", types_[0].GetBaseType_().GetBits()
            ), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new TextPatternSegment("|"),
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("|")
                ], new SlotPatternProcessor([1])
            ), [
                new("value", new Type_("Q", 64))
            ], "builtin15", (List<Type_> types_) => new Type_(
                "Q", types_[0].GetBaseType_().GetBits()
            ), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("+"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 2])
            ), [
                new("array1", Type_.Any().ArrayOf()),
                new("array2", Type_.Any().ArrayOf()),
            ], "builtin16", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot concat array of type {types_[0]} with an array of type {types_[1]}", 1);
                return types_[0];
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "stringify")
                ], new SlotPatternProcessor([0])
            ), [
                new("value", Type_.Any()),
            ], "builtin17", Type_.String(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "print"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([1])
            ), [
                new("value", Type_.Any()),
            ], "builtin18", FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "println"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([1])
            ), [
                new("value", Type_.Any()),
            ], "builtin19", FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "left_pad"),
                    new FuncArgPatternSegment(),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3, 4])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("len", new Type_("W", 64)),
                new("val", Type_.Any()),
            ], "builtin20", (List<Type_> types_) => {
                Type_ generic = types_[0].GetGeneric(0);
                if (!types_[2].IsConvertibleTo(generic))
                    throw new FunctionCallTypes_Exception($"Cannot pad array of type {types_[1]} with a value of type {types_[0]}", 2);
                return null;
            }, FunctionSource.Builtin, doesReturnVoid: true
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "right_pad"),
                    new FuncArgPatternSegment(),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3, 4])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("len", new Type_("W", 64)),
                new("val", Type_.Any()),
            ], "builtin21", (List<Type_> types_) => {
                Type_ generic = types_[0].GetGeneric(0);
                if (!types_[2].IsConvertibleTo(generic))
                    throw new FunctionCallTypes_Exception($"Cannot pad array of type {types_[1]} with a value of type {types_[0]}", 2);
                return null;
            }, FunctionSource.Builtin, doesReturnVoid: true
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "slice"),
                    new FuncArgPatternSegment(),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3, 4])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("start", new Type_("W", 64)),
                new("end", new Type_("W", 64)),
            ], "builtin22", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "count"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("array", Type_.Any().ArrayOf()),
                new("elem", Type_.Any()),
            ], "builtin23", (List<Type_> types_) => {
                if (!types_[1].IsConvertibleTo(types_[0].GetGeneric(0)) && !types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot count occurrences of element or subarray of type {types_[1]} in an array of type {types_[0]}", 1);
                return new Type_("W", 64);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "overlap_count"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("sub", Type_.Any().ArrayOf()),
            ], "builtin24", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot count occurrences of array of type {types_[1]} in an array of type {types_[0]}", 1);
                return new Type_("W", 64);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "pop_end")
                ], new SlotPatternProcessor([0])
            ), [
                new("array", Type_.Any().ArrayOf())
            ], "builtin25", (List<Type_> types_) => {
                return types_[0].GetGeneric(0);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "nest"),
                ], new SlotPatternProcessor([0])
            ), [
                new("arr", Type_.Any().ArrayOf()),
            ], "builtin26", (List<Type_> types_) => types_[0].ArrayOf(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "split"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("sub", Type_.Any().ArrayOf()),
            ], "builtin27", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot split array of type {types_[0]} by an array of type {types_[1]}", 1);
                return types_[0].ArrayOf();
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "starts_with"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("sub", Type_.Any().ArrayOf()),
            ], "builtin28", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot check if array of type {types_[0]} starts with array of type {types_[1]}", 1);
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "ends_with"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("sub", Type_.Any().ArrayOf()),
            ], "builtin29", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot check if array of type {types_[0]} ends with array of type {types_[1]}", 1);
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new UnitPatternSegment<string>(typeof(Name), "equals"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 2])
            ), [
                new("v1", Type_.Any()),
                new("v2", Type_.Any()),
            ], "builtin30", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"The 'equals' function can only compare equality of values of equal types_", 1);
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new UnitPatternSegment<string>(typeof(Name), "not"),
                    new UnitPatternSegment<string>(typeof(Name), "equals"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("v1", Type_.Any()),
                new("v2", Type_.Any()),
            ], "builtin31", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"The 'not equals' function can only compare equality of values of equal types_", 1);
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new UnitPatternSegment<string>(typeof(Name), "deep"),
                    new UnitPatternSegment<string>(typeof(Name), "equals"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("v1", Type_.Any()),
                new("v2", Type_.Any())
            ], "builtin32", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1])) {
                    throw new FunctionCallTypes_Exception($"The 'deep equals' function can only compare equality of values of equal types_", 1);
                }
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new UnitPatternSegment<string>(typeof(Name), "deep"),
                    new UnitPatternSegment<string>(typeof(Name), "not"),
                    new UnitPatternSegment<string>(typeof(Name), "equals"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 4])
            ), [
                new("v1", Type_.Any()),
                new("v2", Type_.Any())
            ], "builtin33", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1])) {
                    throw new FunctionCallTypes_Exception($"The 'deep not equals' function can only compare equality of values of equal types_", 1);
                }
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "join"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("arr", Type_.Any().ArrayOf().ArrayOf()),
                new("sep", Type_.Any().ArrayOf()),
            ], "builtin34", (List<Type_> types_) => {
                if (!types_[0].GetGeneric(0).Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot join of array of type {types_[0]} on array of type {types_[1]}", 1);
                return types_[1];
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "index_of"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("elem", Type_.Any()),
            ], "builtin35", (List<Type_> types_) => {
                if (!types_[1].IsConvertibleTo(types_[0].GetGeneric(0)) && !types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot check index of element or array of type {types_[1]} in array of type {types_[0]}", 1);
                return new Type_("Z", 64);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "parse_int"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([1])
            ), [
                new("str", Type_.String()),
            ], "builtin36", new Type_("Z", 32), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "invalid_parsed_int"),
                ], new EmptyPatternProcessor()
            ), [], "builtin37", new Type_("Z", 32), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "parse_float"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([1])
            ), [
                new("str", Type_.String()),
            ], "builtin38", new Type_("Q", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "is_NaN"),
                    new FuncArgPatternSegment(),
                ], new SlotPatternProcessor([1])
            ), [
                new("val", new Type_("Q", 64)),
            ], "builtin39", new Type_("Bool"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "read_input_line"),
                ], new EmptyPatternProcessor()
            ), [], "builtin40", Type_.String(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "is_null")
                ], new SlotPatternProcessor([0])
            ), [
                new("nullable", Type_.Any()),
            ], "builtin41", (List<Type_> types_) => {
                BaseType_ bt = types_[0].GetBaseType_();
                if (bt.IsNull()) {
                    throw new FunctionCallTypes_Exception(
                        "Value is always null", 0
                    );
                }
                if (!bt.IsNullable()) {
                    throw new FunctionCallTypes_Exception(
                        $"Type {types_[0]} is never null", 0
                    );
                }
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "unwrap")
                ], new SlotPatternProcessor([0])
            ), [
                new("optional", Type_.Any().OptionalOf(), exactType_Match: true),
            ], "builtin42", (List<Type_> types_) => types_[0].GetGeneric(0),
            FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "sort")
                ], new SlotPatternProcessor([0])
            ), [
                new("array", Type_.Any().OptionalOf()),
            ], "builtin43", (List<Type_> types_) => {
                if (!types_[0].GetGeneric(0).GetBaseType_().IsNumber()) {
                    throw new FunctionCallTypes_Exception(
                        $"Only arrays of numbers can be sorted, not {types_[0]}", 0
                    );
                }
                return null;
            }, FunctionSource.Builtin, doesReturnVoid: true
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "sort_inverted")
                ], new SlotPatternProcessor([0])
            ), [
                new("array", Type_.Any().ArrayOf()),
            ], "builtin44", (List<Type_> types_) => {
                if (!types_[0].GetGeneric(0).GetBaseType_().IsNumber()) {
                    throw new FunctionCallTypes_Exception(
                        $"Only arrays of numbers can be sorted, not {types_[0]}", 0
                    );
                }
                return null;
            }, FunctionSource.Builtin, doesReturnVoid: true
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "dedup")
                ], new SlotPatternProcessor([0])
            ), [
                new("array", Type_.Any().ArrayOf()),
            ], "builtin45", FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "unique")
                ], new SlotPatternProcessor([0])
            ), [
                new("array", Type_.Any().ArrayOf()),
            ], "builtin46", FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "repeat"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("array", Type_.Any().ArrayOf()),
                new("times", new Type_("W", 64))
            ], "builtin47", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "truthy")
                ], new SlotPatternProcessor([0])
            ), [
                new("value", Type_.Any())
            ], "builtin48", new Type_("Bool"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "floor")
                ], new SlotPatternProcessor([0])
            ), [
                new("value", new Type_("Q"))
            ], "builtin49", new Type_("Z"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "ceil")
                ], new SlotPatternProcessor([0])
            ), [
                new("value", new Type_("Q"))
            ], "builtin50", new Type_("Z"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "round")
                ], new SlotPatternProcessor([0])
            ), [
                new("value", new Type_("Q"))
            ], "builtin51", new Type_("Z"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "inner")
                ], new SlotPatternProcessor([0])
            ), [
                new("optional", Type_.Any().OptionalOf(), exactType_Match: true),
            ], "builtin52", (List<Type_> types_) => types_[0].GetGeneric(0),
            FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("|"),
                    new TextPatternSegment("|"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("a", Type_.Any().OptionalOf(), exactType_Match: true),
                new("b", Type_.Any()),
            ], "builtin53", (List<Type_> types_) => {
                if (types_[1].GetBaseType_().GetName() == "Optional") {
                    if (!types_[1].IsConvertibleTo(types_[0])) {
                        throw new FunctionCallTypes_Exception(
                            $"Cannot convert {types_[1]} to {types_[0]}", 1
                        );
                    }
                    return types_[0];
                } else {
                    if (!types_[1].IsConvertibleTo(types_[0].GetGeneric(0))) {
                        throw new FunctionCallTypes_Exception(
                            $"Cannot convert {types_[1]} to {types_[0].GetGeneric(0)}", 1
                        );
                    }
                    return types_[0].GetGeneric(0);
                }
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("&"),
                    new TextPatternSegment("&"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("a", Type_.Any().OptionalOf(), exactType_Match: true),
                new("b", Type_.Any()),
            ], "builtin54", (List<Type_> types_) => {
                if (types_[1].GetBaseType_().GetName() == "Optional") {
                    return types_[1];
                } else {
                    return types_[1].OptionalOf();
                }
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new TextPatternSegment("~"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([1])
            ), [
                new("a", new Type_("Z"))
            ], "builtin55", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("&"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 2])
            ), [
                new("a", new Type_("Z")),
                new("b", new Type_("Z")),
            ], "builtin56", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("|"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 2])
            ), [
                new("a", new Type_("Z")),
                new("b", new Type_("Z")),
            ], "builtin57", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("^"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 2])
            ), [
                new("a", new Type_("Z")),
                new("b", new Type_("Z")),
            ], "builtin58", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("<"),
                    new TextPatternSegment("<"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("a", new Type_("Z")),
                new("b", new Type_("Z")),
            ], "builtin59", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment(">"),
                    new TextPatternSegment(">"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("a", new Type_("Z")),
                new("b", new Type_("Z")),
            ], "builtin60", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "unsafe"),
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("<"),
                    new TextPatternSegment("<"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([1, 4])
            ), [
                new("a", new Type_("Z")),
                new("b", new Type_("Z")),
            ], "builtin61", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "unsafe"),
                    new FuncArgPatternSegment(),
                    new TextPatternSegment(">"),
                    new TextPatternSegment(">"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([1, 4])
            ), [
                new("a", new Type_("Z")),
                new("b", new Type_("Z")),
            ], "builtin62", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new UnitPatternSegment<string>(typeof(Name), "unsafe"),
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("%"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([1, 3])
            ), [
                new("a", new Type_("Z")),
                new("b", new Type_("Z")),
            ], "builtin63", (List<Type_> types_) => {
                Type_ common = Type_.CommonOrNull(types_[0], types_[1]);
                if (common == null) {
                    throw new FunctionCallTypes_Exception(
                        "Incompatible types for modulo", 0
                    );
                }
                return common;
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "at"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("idx", new Type_("Z", 64)),
            ], "builtin64", (List<Type_> types_) => types_[0].GetGeneric(0), FunctionSource.Builtin
        ),  new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "at"),
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("="),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3, 5])
            ), [
                new("arr", Type_.Any().ArrayOf()),
                new("idx", new Type_("Z", 64)),
                new("val", Type_.Any())
            ], "builtin65", (List<Type_> types_) => {
                if (!types_[2].IsConvertibleTo(types_[0].GetGeneric(0)))
                    throw new FunctionCallTypes_Exception($"Cannot add value of type {types_[2]} to array of type {types_[0]}", 2);
                return null;
            }, FunctionSource.Builtin, doesReturnVoid: true
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                [
                    new FuncArgPatternSegment(),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "expect"),
                    new FuncArgPatternSegment()
                ], new SlotPatternProcessor([0, 3])
            ), [
                new("optional", Type_.Any().OptionalOf(), exactType_Match: true),
                new("fail_msg", Type_.String())
            ], "builtin66", (List<Type_> types_) => types_[0].GetGeneric(0),
            FunctionSource.Builtin
        ),
    ];
}
