using System;

public class AccountOne
{
	private Decimal _balance;

	public AccountOne(Decimal startingBalance)
	{
		_balance = startingBalance;
	}

	public Decimal Balance
	{
		get 
		{
			return _balance;
		}
	}

	public void Deposit(Decimal amount)
	{
		_balance += amount;
	}

	public void Withdraw(Decimal amount)
	{
		_balance -= amount;
	}
}