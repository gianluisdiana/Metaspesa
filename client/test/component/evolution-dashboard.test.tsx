/* @vitest-environment jsdom */
import ChartCard from '@/app/(protected)/evolution/components/chart-card';
import DataTable from '@/app/(protected)/evolution/components/data-table';
import { cleanup, render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it } from 'vitest';

function renderEvolutionDashboard() {
  render(
    <>
      <ChartCard />
      <DataTable />
    </>,
  );
}

describe('evolution dashboard components', () => {
  afterEach(cleanup);

  it('renders price history chart heading', () => {
    renderEvolutionDashboard();

    expect(screen.getByText('Price & Volume Trend')).toBeVisible();
  });

  it('renders historical data table heading', () => {
    renderEvolutionDashboard();

    expect(screen.getByText('Historical Data Points')).toBeVisible();
  });

  it('renders historical market row', () => {
    renderEvolutionDashboard();

    expect(screen.getByText("Trader Joe's")).toBeVisible();
  });

  it('renders export action', () => {
    renderEvolutionDashboard();

    expect(screen.getByRole('button', { name: /export csv/i })).toBeVisible();
  });
});
