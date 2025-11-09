export type ParsedError = {
	title?: string;
	message?: string;
	errors?: Record<string, string[]> | string[];
	[key: string]: unknown;
};
