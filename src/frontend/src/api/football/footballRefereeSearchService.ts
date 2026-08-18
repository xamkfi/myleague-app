import { footballRefereeService, type FootballRefereeDto } from "./footballRefereeService";

export interface DropdownOption {
  id: string;
  name: string;
  [key: string]: unknown; // Allow additional properties
}

export interface SearchResult {
  data: DropdownOption[];
  pagination: {
    hasNextPage: boolean;
    totalCount: number;
  };
}

export const footballRefereeSearchService = {
    searchReferees: async (query: string, page: number): Promise<SearchResult> => {
        try {
            const response = await footballRefereeService.getAll({
                page,
                pageSize: 50, //Use max allowed for referees
            });

            if (!response.success || !response.data) {
                throw new Error('Failed to fetch referees');
            }

            //Convert referees to dropdown options
            let referees: DropdownOption[] = response.data.map((referee: FootballRefereeDto) => ({
                id: referee.id,
                name: referee.person.fullName,
            }));

            if (query.trim()) {
                referees = referees.filter(referee =>
                    referee.name.toLowerCase().includes(query.toLowerCase())
                );
            }

            return {
                data: referees,
                pagination: {
                    hasNextPage: response.pagination?.hasNextPage || false,
                    totalCount: response.pagination?.totalCount || referees.length,
                },
            };
        } catch (error) {
            throw new Error(error instanceof Error ? error.message : 'Failed to search referees');
        }
    }
}