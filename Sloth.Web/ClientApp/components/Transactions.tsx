import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import * as api from '../api';

interface TransactionsState {
    transactions: api.Transaction[];
    loading: boolean;
}

export class Transactions extends React.Component<RouteComponentProps<{}>, TransactionsState> {


    constructor() {
        super();
        this.state = { transactions: [], loading: true };

        const options = { headers: { 'x-auth-token': 'TestKey123' } };
        const client = new api.TransactionsApi();
        client.basePath = 'http://sloth-api-test.azurewebsites.net';
        client.v1TransactionsGet(options)
            .then(data => {
                this.setState({ transactions: data, loading: false });
            });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Transactions.renderTable(this.state.transactions);

        return <div>
            <h1>Weather forecast</h1>
            <p>This component demonstrates fetching data from the server.</p>
            { contents }
        </div>;
    }

    private static renderTable(transactions: api.Transaction[]) {
        return <table className='table'>
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Date</th>
                    <th>Document Number</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
            {transactions.map(t =>
                <tr key={ t.id }>
                    <td>{ t.id }</td>
                    <td>{ t.transactionDate }</td>
                    <td>{ t.documentNumber }</td>
                    <td>{ t.status }</td>
                </tr>
            )}
            </tbody>
        </table>;
    }
}
