using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.LinqMapper
{
    internal interface IExpressionBuilder
    {
        void ExtendWith(IExpressionBuilder other);
        void RemoveMembers(string[] members);
        IExpressionBuilder CastTo<TSourceChild, TDestChild>()
            where TDestChild : class, new();

        LambdaExpression Build();
    }

    internal class ExpressionBuilder<TSource, TDest> : IExpressionBuilder
        where TDest : class, new()
    {
        private Expression<Func<TSource, TDest>> _expression;
        private ParameterExpression _SourceParam = null;
        private Dictionary<string, MemberAssignment> _PropertyBindings = null;

        private ExpressionBuilder() { }

        public ExpressionBuilder(Expression<Func<TSource, TDest>> exp)
            : this()
        {
            if (exp == null)
                throw new ArgumentNullException("exp");

            _expression = exp;
        }


        private void ResetExpression()
        {
            this.GetSourceParam();
            this.GetPropertyBindings();

            _expression = null;
        }

        private ParameterExpression GetSourceParam()
        {
            if (this._SourceParam != null || _expression == null)
                return this._SourceParam;

            return (this._SourceParam = _expression.Parameters[0]);
        }

        private Dictionary<string, MemberAssignment> GetPropertyBindings()
        {
            if (this._PropertyBindings != null || _expression == null)
                return this._PropertyBindings;

            return (this._PropertyBindings = LinqMapper.GetBindingExpressionsDictionary(_expression));
        }

        public void ExtendWith(ExpressionBuilder<TSource, TDest> other)
        {
            if (other == null)
                return;

            var otherPropertyBindings = other.GetPropertyBindings();
            if (otherPropertyBindings.Count == 0)
                return;

            var sourceParamExp = this.GetSourceParam();
            var otherParamExp = other.GetSourceParam();
            var sourceReplacer = new SourceParamReplacer(otherParamExp, sourceParamExp);
            var thisPropertyBindings = this.GetPropertyBindings();

            foreach (var runtimeBinding in otherPropertyBindings)
            {
                var replaced = VisitMember(runtimeBinding.Value, sourceReplacer);
                thisPropertyBindings[runtimeBinding.Key] = replaced;
            }

            this.ResetExpression(); //delayed exp creation
        }

        internal static MemberAssignment VisitMember(MemberAssignment m, ExpressionVisitor visitor)
        {
            var exp = visitor.Visit(m.Expression);
            if (Object.ReferenceEquals(exp, m.Expression))
                return m;

            return m.Update(exp);
        }

        public void RemoveMembers(string[] members)
        {
            if (members == null || members.Length == 0)
                return;

            var propertyBindings = this.GetPropertyBindings();
            if (propertyBindings.Count == 0) // nothing to remove
                return;

            var removed = false;
            foreach (var member in members)
            {
                if (propertyBindings.Remove(member))
                    removed = true;
            }

            if (removed)
            {
                this.ResetExpression(); //delayed exp creation            
            }
        }

        public ExpressionBuilder<TSourceChild, TDestChild> CastTo<TSourceChild, TDestChild>()
            where TDestChild : class, new()
        {
            var childSourceType = typeof(TSourceChild);
            var sourceParam = this.GetSourceParam();
            var propertyBindings = this.GetPropertyBindings();

            var newParamExp = (childSourceType == typeof(TSource)) ? sourceParam : Expression.Parameter(childSourceType, sourceParam.Name);
            var castVisitor = new CastMapVisitor<TSource, TDest, TSourceChild, TDestChild>(sourceParam, newParamExp);

            var newPropertyBindings = new Dictionary<string, MemberAssignment>(propertyBindings.Count);
            foreach (var kvp in propertyBindings)
            {
                var castMember = VisitMember(kvp.Value, castVisitor);
                newPropertyBindings.Add(kvp.Key, castMember);
            }

            return new ExpressionBuilder<TSourceChild, TDestChild>()
            {
                _expression = null, //delayed exp creation
                _SourceParam = sourceParam,
                _PropertyBindings = newPropertyBindings
            };
        }

        public Expression<Func<TSource, TDest>> Build()
        {
            if (_expression != null || this._PropertyBindings == null || this._SourceParam == null)
                return _expression;

            var newExp = ExpressionNewCreator<TDest>.NewExpression;
            var body = (Expression)Expression.MemberInit(newExp, this._PropertyBindings.Values);

            return (_expression = Expression.Lambda<Func<TSource, TDest>>(body, this._SourceParam));
        }

        void IExpressionBuilder.ExtendWith(IExpressionBuilder other)
        {
            this.ExtendWith((ExpressionBuilder<TSource, TDest>)other);
        }

        LambdaExpression IExpressionBuilder.Build()
        {
            return this.Build();
        }

        void IExpressionBuilder.RemoveMembers(string[] members)
        {
            this.RemoveMembers(members);
        }

        IExpressionBuilder IExpressionBuilder.CastTo<TSourceChild, TDestChild>()
        {
            return this.CastTo<TSourceChild, TDestChild>();
        }

        internal static IEnumerable<MemberAssignment> MergePropertyBindings(IReadOnlyDictionary<string, MemberAssignment> staticBindings, IReadOnlyDictionary<string, MemberAssignment> runtimeBindings)
        {
            foreach (var staticBinding in staticBindings)
            {
                if (runtimeBindings.ContainsKey(staticBinding.Key))
                    continue;

                yield return staticBinding.Value;
            }

            foreach (var runtimeBinding in runtimeBindings)
            {
                yield return runtimeBinding.Value;
            }
        }
    }

    internal class SourceParamReplacer : System.Linq.Expressions.ExpressionVisitor
    {
        ParameterExpression _parameter;
        ParameterExpression _replacefor;

        public SourceParamReplacer(ParameterExpression parameter, ParameterExpression replacefor)
        {
            _parameter = parameter;
            _replacefor = replacefor;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (Object.ReferenceEquals(node, _parameter))
                return _replacefor;

            return node;
        }

        public override Expression Visit(Expression node)
        {
            if (Object.ReferenceEquals(_parameter, _replacefor))
            {
                return node; //no need to replace, parameter is the same
            }
            return base.Visit(node);
        }
    }

    internal class CastMapVisitor<TBaseSource, TBaseDest, TSource, TDest> : SourceParamReplacer
    {
        Type _oldInitType;

        public CastMapVisitor(ParameterExpression oldParam, ParameterExpression newParam)
            : base(oldParam, newParam)
        {
            _oldInitType = typeof(TBaseDest);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Type == _oldInitType)
                return ExpressionNewCreator<TDest>.NewExpression;

            return base.VisitNew(node);
        }

        private IEnumerable<ParameterExpression> _visitParameters(IReadOnlyList<ParameterExpression> parameters)
        {
            var length = parameters.Count;
            for (int i = 0; i < length; i++)
            {
                yield return (ParameterExpression)this.Visit(parameters[i]);
            }
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var parameters = _visitParameters(node.Parameters).ToArray();
            var body = this.Visit(node.Body);

            var lambda = Expression.Lambda(body, parameters);
            return lambda;
        }
    }
}
