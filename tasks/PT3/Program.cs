using System;
using System.Threading;

public class AccountTest
{
	public static AccountThree _accountA = new AccountThree(1);
	public static AccountThree _accountB = new AccountThree(100);

	public static void DepositMeMillions()
	{
		for (int i = 0; i < 1000000; i++)
		{
			_accountA.Deposit(1);
		}
	}

	public static void WithdrawMeMillions()
	{
		for (int i = 0; i < 1000000; i++)
		{
			_accountA.Withdraw(1);
		}
	}

	public static void TestTransfer(Decimal amount)
	{
		_accountB.Transfer(_accountA, amount);
	}

	public static void Main ()
	{
		Thread[] t = new Thread[] {
			new Thread (DepositMeMillions),
			new Thread (WithdrawMeMillions)
		};

		TestTransfer(30);

		t[0].Name = "Thread 1";
		t[1].Name = "Thread 2";

		t[0].Start();
		t[1].Start();

		t[0].Join();
		t[1].Join();

		Console.WriteLine("Balance for account a is now {0}", _accountA.Balance);
		Console.WriteLine("Balance for account b is now {0}", _accountB.Balance);
	}
}
