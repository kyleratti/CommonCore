﻿using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Dapper;
using FruityFoundation.DataAccess.Abstractions;
using Microsoft.Data.Sqlite;

namespace FruityFoundation.DataAccess.Sqlite;

// ReSharper disable once UnusedTypeParameter
public class NonTransactionalDbConnection<TConnectionType> : SqliteConnection, INonTransactionalDbConnection<TConnectionType>
	where TConnectionType : ConnectionType
{
	/// <summary>
	/// C'tor
	/// </summary>
	public NonTransactionalDbConnection(string connectionString) : base(connectionString)
	{
	}

	/// <inheritdoc />
	public async Task<IEnumerable<T>> Query<T>(
		string sql,
		object? param = null,
		CancellationToken cancellationToken = default
	) => await this.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: cancellationToken));

	/// <inheritdoc />
	public async IAsyncEnumerable<T> QueryUnbuffered<T>(
		string sql,
		object? param = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		var query = this.QueryUnbufferedAsync<T>(sql, param, transaction: null)
			.WithCancellation(cancellationToken);

		await foreach (var item in query)
			yield return item;
	}

	/// <inheritdoc />
	public async Task<T> QuerySingle<T>(
		string sql,
		object? param = null,
		CancellationToken cancellationToken = default
	) => await this.QuerySingleAsync<T>(new CommandDefinition(sql, param, cancellationToken: cancellationToken));

	/// <inheritdoc />
	public async Task Execute(
		string sql,
		object? param = null,
		CancellationToken cancellationToken = default
	) => await this.ExecuteAsync(new CommandDefinition(sql, param, cancellationToken: cancellationToken));

	/// <inheritdoc />
	public async Task<T?> ExecuteScalar<T>(
		string sql,
		object? param = null,
		CancellationToken cancellationToken = default
	) => await this.ExecuteScalarAsync<T>(new CommandDefinition(sql, param, cancellationToken: cancellationToken));

	/// <inheritdoc />
	public async Task<DbDataReader> ExecuteReader(
		string sql,
		object? param = null,
		CancellationToken cancellationToken = default
	) => await this.ExecuteReaderAsync(new CommandDefinition(sql, param, cancellationToken: cancellationToken));

	/// <inheritdoc />
	public async Task<IDatabaseTransactionConnection<TConnectionType>> CreateTransaction()
	{
		if (!State.HasFlag(ConnectionState.Open))
			await OpenAsync();

		var tx = BeginTransaction();

		return new DbTransaction<TConnectionType>(tx);
	}

	/// <inheritdoc />
	public async Task<IDatabaseTransactionConnection<TConnectionType>> CreateTransaction(IsolationLevel isolationLevel)
	{
		if (!State.HasFlag(ConnectionState.Open))
			await OpenAsync();

		var tx = BeginTransaction(isolationLevel);

		return new DbTransaction<TConnectionType>(tx);
	}

	/// <inheritdoc />
	public async Task<IDatabaseTransactionConnection<TConnectionType>> CreateTransaction(IsolationLevel isolationLevel, bool deferred)
	{
		if (!State.HasFlag(ConnectionState.Open))
			await OpenAsync();

		var tx = BeginTransaction(isolationLevel, deferred);

		return new DbTransaction<TConnectionType>(tx);
	}
}
