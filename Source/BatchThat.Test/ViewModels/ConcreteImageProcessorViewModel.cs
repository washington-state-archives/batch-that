using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using BatchThat.Image.Filters;
using BatchThat.ViewModels;

namespace BatchThat.Test.ViewModels
{
    public class ConcreteImageProcessorViewModel : ImageProcessorViewModel
    {
        public string Field;

        public new IList<Filter> GetFilters()
        {
            return base.GetFilters();
        }

        public new PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            return base.GetPropertyInfo(source, propertyLambda);
        }
    }
}