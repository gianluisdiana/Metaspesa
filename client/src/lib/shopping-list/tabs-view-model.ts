import {
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

const EMPTY_LIST_SORT_ORDER = -1;
const NAMED_LIST_SORT_ORDER = 1;

export class ShoppingListTabViewModel {
  public constructor(
    private readonly summary: ShoppingListSummaryMessage,
    private readonly selectedListName?: string,
  ) {}

  public get active(): boolean {
    return this.name === this.selectedListName;
  }

  public get label(): string {
    return this.name && this.name.length > 0 ? this.name : 'Temporary List';
  }

  public get name(): string | undefined {
    return this.summary.name && this.summary.name.length > 0
      ? this.summary.name
      : undefined;
  }
}

export class ShoppingListTabsViewModel {
  public constructor(
    private readonly summaries: ShoppingListSummaryMessage[],
    private readonly selectedListName?: string,
    private readonly fallbackList?: ShoppingListMessage,
  ) {}

  public get tabs(): ShoppingListTabViewModel[] {
    return this.normalizedSummaries
      .toSorted((a, b) => {
        if (!a.name) return EMPTY_LIST_SORT_ORDER;
        if (!b.name) return NAMED_LIST_SORT_ORDER;
        return a.name.localeCompare(b.name);
      })
      .map(
        summary => new ShoppingListTabViewModel(summary, this.selectedListName),
      );
  }

  private get normalizedSummaries(): ShoppingListSummaryMessage[] {
    if (this.summaries.length > 0) {
      return this.summaries;
    }

    return [{ name: this.fallbackList?.name }];
  }
}
