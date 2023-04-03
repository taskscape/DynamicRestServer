using Dynamic.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Linq;

namespace Dynamic.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal class BaseImplemetation : Attribute, IActionConstraint
    {
        public bool IsBaseImplementation { get; }

        public BaseImplemetation(bool isBaseImplementation = false)
        {
            IsBaseImplementation = isBaseImplementation;
        }

        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            if (context.Candidates.Count == 1)
            {
                return true;
            }

            if (context.Candidates.All(x => x.Action is ControllerActionDescriptor controllerAction && !controllerAction.MethodInfo.IsOverride()))
            {
                if (((ControllerActionDescriptor)context.CurrentCandidate.Action).ControllerTypeInfo.BaseType == typeof(ControllerBase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            foreach (var item in context.Candidates.Where(x => !x.Equals(context.CurrentCandidate)))
            {
                var attr = item.Action.ActionConstraints.FirstOrDefault(x => x is BaseImplemetation);

                if (attr is BaseImplemetation impl)
                {
                    return impl.IsBaseImplementation;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }
    }
}
