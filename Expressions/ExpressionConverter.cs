using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace DotExperiments
{
	public static class ExpressionUtility
	{
		public static Expression<T> Convert<T>([CanBeNull] this Expression expr)
			where T : Delegate
		{
			Expression<T> e = expr switch
			{
				null => null,
				LambdaExpression lambda => new ExpressionConverter<T>(lambda).VisitAndConvert(),
				_ => throw new ArgumentException("Can't convert a non lambda.")
			};

			return ExpressionRewrite.Rewrite<T>(e);
		}

		private class ExpressionConverter<TTo> : ExpressionVisitor
			where TTo : Delegate
		{
			private readonly LambdaExpression _expression;
			private readonly ParameterExpression[] _newParams;

			internal ExpressionConverter(LambdaExpression expression)
			{
				_expression = expression;

				Type[] paramTypes = typeof(TTo).GetGenericArguments()[..^1];
				if (paramTypes.Length != _expression.Parameters.Count)
					throw new ArgumentException("Parameter count from internal and external lambda are not matched.");

				_newParams = new ParameterExpression[paramTypes.Length];
				for (int i = 0; i < paramTypes.Length; i++)
				{
					if (_expression.Parameters[i].Type == paramTypes[i])
						_newParams[i] = _expression.Parameters[i];
					else
						_newParams[i] = Expression.Parameter(paramTypes[i], _expression.Parameters[i].Name);
				}
			}

			internal Expression<TTo> VisitAndConvert()
			{
				Type returnType = _expression.Type.GetGenericArguments().Last();
				Expression body = _expression.ReturnType == returnType
					? Visit(_expression.Body)
					: Expression.Convert(Visit(_expression.Body)!, returnType);
				return Expression.Lambda<TTo>(body!, _newParams);
			}

			protected override Expression VisitParameter(ParameterExpression node)
			{
				return _newParams.FirstOrDefault(x => x.Name == node.Name) ?? node;
			}
		}
	}
}
