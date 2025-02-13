﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bitter.Tools.ExpressionUtil
{
    [ExcludeFromCodeCoverage]
    internal sealed class HoistingExpressionVisitor<TIn, TOut>: ExpressionVisitor
    {
        private static readonly ParameterExpression _hoistedConstantsParamExpr = Expression.Parameter(typeof(List<object>), "hoistedConstants");
        private int _numConstantsProcessed;

        // factory will create instance
        private HoistingExpressionVisitor()
        {
        }

        public static Expression<Hoisted<TIn, TOut>> Hoist(Expression<Func<TIn, TOut>> expr)
        {
            // rewrite Expression<Func<TIn, TOut>> as Expression<Hoisted<TIn, TOut>>

            var visitor = new HoistingExpressionVisitor<TIn, TOut>();
            var rewrittenBodyExpr = visitor.Visit(expr.Body);
            var rewrittenLambdaExpr = Expression.Lambda<Hoisted<TIn, TOut>>(rewrittenBodyExpr, expr.Parameters[0], _hoistedConstantsParamExpr);
            return rewrittenLambdaExpr;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // rewrite the constant expression as (TConst)hoistedConstants[i];
            // coverity[missing_super_call]
            return Expression.Convert(Expression.Property(_hoistedConstantsParamExpr, "Item", Expression.Constant(_numConstantsProcessed++)), node.Type);
        }
    }
}
