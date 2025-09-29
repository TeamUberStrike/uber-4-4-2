using System;

public class Property<T>
{
	private T currentValue;

	public virtual T Value
	{
		get
		{
			return currentValue;
		}
		set
		{
			T oldValue = currentValue;
			currentValue = value;
			Fire(oldValue);
		}
	}

	public event Action<T> Changed;

	public event Action<T, T> ChangedWithPrev;

	public Property()
	{
	}

	public Property(T defaultValue)
	{
		currentValue = defaultValue;
	}

	public void AddEventAndFire(Action<T> onChanged)
	{
		this.Changed = (Action<T>)Delegate.Combine(this.Changed, onChanged);
		onChanged(currentValue);
	}

	public void AddEventAndFire(Action<T, T> onChanged)
	{
		this.ChangedWithPrev = (Action<T, T>)Delegate.Combine(this.ChangedWithPrev, onChanged);
		onChanged(currentValue, currentValue);
	}

	public void Fire()
	{
		Fire(currentValue);
	}

	public void Fire(T oldValue)
	{
		if (this.Changed != null)
		{
			this.Changed(currentValue);
		}
		if (this.ChangedWithPrev != null)
		{
			this.ChangedWithPrev(currentValue, oldValue);
		}
	}

	public override string ToString()
	{
		return currentValue.ToString();
	}

	public static implicit operator T(Property<T> property)
	{
		return property.Value;
	}
}
