using Microsoft.JSInterop;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Sorting;

public class DynamicJSRuntime : IDynamicMetaObjectProvider
{
    readonly IJSRuntime runtime;
    readonly string prefix;

    public DynamicJSRuntime(IJSRuntime runtime) : this(runtime, string.Empty) { }
    DynamicJSRuntime(IJSRuntime runtime, string prefix) { (this.runtime, this.prefix) = (runtime, prefix); }

    public DynamicMetaObject GetMetaObject(Expression parameter) => new MetaObject(parameter, this);

    private string Qualify(string path)
    {
        if (string.IsNullOrEmpty(this.prefix)) return path;

        return $"{this.prefix}.{path}";
    }

    private DynamicJSRuntime WithPrefix(string prefix)
    {
        return new DynamicJSRuntime(this.runtime, this.Qualify(prefix));
    }
    private Task<T> InvokeAsync<T>(string identity, object[] args) => this.runtime.InvokeAsync<T>(this.Qualify(identity), args).AsTask();
    private Task<T> InvokeAsync<T>(object[] args) => this.runtime.InvokeAsync<T>(this.prefix, args).AsTask();

    class MetaObject : DynamicMetaObject
    {
        readonly Expression runtimeExpr;
        readonly MethodInfo invokeMethod;
        readonly MethodInfo invokeSelfMethod;
        readonly MethodInfo withPrefixMethod;

        internal MetaObject(Expression expression, DynamicJSRuntime runtime) : this(expression, BindingRestrictions.Empty, runtime) { }

        private MetaObject(Expression expression, BindingRestrictions restrictions, DynamicJSRuntime dynamicRuntime)
             : base(expression, restrictions, dynamicRuntime)
        {
            this.runtimeExpr = this.Limit(Expression.Convert(this.Expression, typeof(DynamicJSRuntime)));
            this.invokeMethod = typeof(DynamicJSRuntime).GetMethod(nameof(DynamicJSRuntime.InvokeAsync), BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(string), typeof(object[]) })!;
            this.invokeSelfMethod = typeof(DynamicJSRuntime).GetMethod(nameof(DynamicJSRuntime.InvokeAsync), BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(object[]) })!;
            this.withPrefixMethod = typeof(DynamicJSRuntime).GetMethod(nameof(DynamicJSRuntime.WithPrefix), BindingFlags.NonPublic | BindingFlags.Instance)!;

        }

        private Expression WithPrefix(Expression expr, string prefix) => Expression.Call(expr, this.withPrefixMethod, Expression.Constant(prefix));
        private Expression Limit(Expression expr) => Expression.Convert(expr, this.LimitType);

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var meta = new DynamicMetaObject(
                    this.WithPrefix(this.runtimeExpr, binder.Name),
                    BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
            return meta;
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            var invokeExpr = Expression.Convert(
                        Expression.Call(
                            this.runtimeExpr,
                            this.invokeMethod.MakeGenericMethod(binder.ReturnType),
                            Expression.Constant(binder.Name),
                            Expression.NewArrayInit(typeof(object), args.Select(arg => arg.Expression))
                        ),
                        binder.ReturnType
                    );

            return new(Expression.Convert(invokeExpr, binder.ReturnType), BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            var invokeExpr = Expression.Convert(
                Expression.Call(
                        this.runtimeExpr,
                        this.invokeSelfMethod.MakeGenericMethod(binder.ReturnType),
                        Expression.NewArrayInit(typeof(object), args.Select(arg => arg.Expression))
                    ),
                    binder.ReturnType
                );

            return new(Expression.Convert(invokeExpr, binder.ReturnType), BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
        }
    }
}