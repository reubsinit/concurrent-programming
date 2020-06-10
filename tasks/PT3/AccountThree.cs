using System;

public class AccountThree
{
	private Decimal _balance;

	public AccountThree(Decimal startingBalance)
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

	public void Transfer(AccountThree toAccount, Decimal amount)
	{

		this.Withdraw(amount);

		toAccount.Deposit(amount);
	}
}