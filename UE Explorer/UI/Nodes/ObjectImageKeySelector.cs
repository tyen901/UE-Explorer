﻿using UELib;
using UELib.Core;
using UELib.Flags;

namespace UEExplorer.UI.Nodes
{
    public class ObjectImageKeySelector : ObjectVisitor<string>
    {
        public override string Visit(IAcceptable visitor)
        {
            string key = base.Visit(visitor);
            return key ?? visitor.GetType().Name;
        }

        public override string Visit(UObject uObject)
        {
            return uObject.GetType().IsSubclassOf(typeof(UProperty)) 
                ? nameof(UProperty) 
                : "Content";
        }

        public override string Visit(UProperty uProperty)
        {
            if (uProperty.HasPropertyFlag(PropertyFlagsLO.ReturnParm)) return "ReturnValue";

            const string key = nameof(UProperty);
            if (uProperty.IsProtected()) return $"{key}-Protected";

            if (uProperty.IsPrivate()) return $"{key}-Private";

            return key;
        }

        public override string Visit(UScriptStruct uScriptStruct)
        {
            return nameof(UStruct);
        }

        public override string Visit(UFunction uFunction)
        {
            string key;
            if (uFunction.HasFunctionFlag(FunctionFlags.Event))
                key = "Event";
            else if (uFunction.HasFunctionFlag(FunctionFlags.Delegate))
                key = "Delegate";
            else if (uFunction.HasFunctionFlag(FunctionFlags.Operator))
                key = "Operator";
            else key = nameof(UFunction);

            if (uFunction.IsProtected()) return $"{key}-Protected";

            if (uFunction.IsPrivate()) return $"{key}-Private";

            return key;
        }

        public override string Visit(UPackage uPackage)
        {
            return "Namespace";
        }

        public override string Visit(UTextBuffer uTextBuffer)
        {
            return "text-left";
        }

        public override string Visit(UClass uClass)
        {
            if (uClass.IsClassInterface()) return "Interface";

            if (uClass.IsClassWithin()) return "UClass-Within";

            return nameof(UClass);
        }
    }
}