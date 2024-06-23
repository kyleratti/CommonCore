﻿using System.Data.Common;
using System.Runtime.CompilerServices;
using Dapper;
using FruityFoundation.DataAccess.Abstractions;
using Microsoft.Data.Sqlite;

namespace FruityFoundation.DataAccess.Sqlite;

public class DbTransaction<TConnectionType> : IDatabaseTransactionConnection<TConnectionType>
	where TConnectionType : ConnectionType
{
	private readonly SqliteTransaction _transaction;

	internal DbTransaction(SqliteTransaction transaction)
	{
		_transaction = transaction;
	}

	/// <exception cref="ArgumentNullException"></exception>
	/// <inheritdoc />
	public async Task<IEnumerable<T>> Query<T>(
		string sql,
		object? param = null,
		CancellationToken cancellationToken = default
	)
	{
		if (_transaction.Connection is not { } conn)
			throw new InvalidOperationException("Transaction connection cannot be null");

		var command = new CommandDefinition(sql, param, transaction: _transaction, cancellationToken: cancellationToken);

		return await conn.QueryAsync<T>(command);
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<T> QueryUnbuffered<T>(
		string sql,
		object? param = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		if (_transaction.Connection is not { } conn)
			throw new InvalidOperationException("Transaction connection cannot be null");

		var query = conn.QueryUnbufferedAsync<T>(sql, param, transaction: _transaction)
			.WithCancellation(cancellationToken);

		await foreach (var item in query)
			yield return item;
	}

	/// <inheritdoc />
	public async Task<T> QuerySingle<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
	{
		if (_transaction.Connection is not { } conn)
			throw new InvalidOperationException("Transaction connection cannot be null");

		var command = new CommandDefinition(sql, param, transaction: _transaction, cancellationToken: cancellationToken);

		return await conn.QuerySingleAsync<T>(command);
	}

	/// <inheritdoc />
	public async Task Execute(string sql, object? param = null, CancellationToken cancellationToken = default)
	{
		if (_transaction.Connection is not { } conn)
			throw new InvalidOperationException("Transaction connection cannot be null");

		var command = new CommandDefinition(sql, param, transaction: _transaction, cancellationToken: cancellationToken);

		await conn.ExecuteAsync(command);
	}

	/// <inheritdoc />
	public async Task<T?> ExecuteScalar<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
	{
		if (_transaction.Connection is not { } conn)
			throw new InvalidOperationException("Transaction connection cannot be null");

		var command = new CommandDefinition(sql, param, transaction: _transaction, cancellationToken: cancellationToken);

		return await conn.ExecuteScalarAsync<T>(command);
	}

	/// <inheritdoc />
	public async Task<DbDataReader> ExecuteReader(
		string sql,
		object? param = null,
		CancellationToken cancellationToken = default
	)
	{
		if (_transaction.Connection is not { } conn)
			throw new InvalidOperationException("Transaction connection cannot be null");

		var command = new CommandDefinition(sql, param, transaction: _transaction, cancellationToken: cancellationToken);

		return await conn.ExecuteReaderAsync(command);
	}

	/// <inheritdoc />
	public async Task Commit(CancellationToken cancellationToken)
	{
		await _transaction.CommitAsync(cancellationToken);
	}

	/// <inheritdoc />
	public async Task Rollback(CancellationToken cancellationToken)
	{
		await _transaction.RollbackAsync(cancellationToken);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_transaction.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await _transaction.DisposeAsync();
		GC.SuppressFinalize(this);
	}
}
