using System;

public class AccountTwo
{
	private Decimal _balance;

	public AccountTwo(Decimal startingBalance)
	{
		_balance = startingBalance;
	}

	public Decimal Balance
	{
		get 
		{
			lock(this)
			{
				return _balance;
			}	
		}
	}

	public void Deposit(Decimal amount)
	{
		lock(this)
		{
			_balance += amount;
		}
	}

	public void Withdraw(Decimal amount)
	{
		lock(this)
		{
			_balance -= amount;
		}	
	}

	public void Transfer(AccountTwo toAccount, Decimal amount)
	{
		lock(this)
		{
			this.Withdraw(amount);
		}

		lock(this)
		{
			toAccount.Deposit(amount);
		}
	}
}