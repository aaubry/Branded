using System;

namespace Branded;

public interface IBrandedType
{
#if NET7_0_OR_GREATER
    static abstract void Dispatch(IBrandedTypeDispatcher dispatcher);
    static abstract Type GetWrappedType();
#endif
}

public interface IBrandedType<TInner> : IBrandedType
{
    TInner Unwrap();
}

public interface IBrandedType<TBranded, TInner> : IBrandedType<TInner>
    where TBranded : IBrandedType<TBranded, TInner>
{
#if NET7_0_OR_GREATER
    static abstract implicit operator TInner(TBranded value);
    static abstract explicit operator TBranded(TInner value);
    static abstract IBrandedValueConverter<TBranded, TInner> GetValueConverter();
#endif
}