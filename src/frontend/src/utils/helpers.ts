import type { RuleItem } from "../types/admin/ruleTypes";
import DOMPurify from "dompurify";

/**
 * Formats a date string to a localized format in UTC (D.M.YYYY)
 * @param dateString Date string to format (can be null)
 * @returns Formatted date string or '-' if null
 */
export function formatDate(dateString: string | null): string {
  if (!dateString) {
    return '-';
  }
  try {
    const date = new Date(dateString);
    const day = date.getUTCDate();
    const month = date.getUTCMonth() + 1; // getUTCMonth is 0-indexed
    const year = date.getUTCFullYear();
    return `${day}.${month}.${year}`;
  } catch (error) {
    console.error('Error formatting date:', error);
    return dateString;
  }
}

/**
 * Formats a date/time string to return date and time in DD/MM HH:MM format.
 * Uses the browser's local timezone (API returns UTC ISO strings).
 *
 * @param dateTime - ISO date string or Date object
 * @returns Array with [date, time] where date is "DD/MM" and time is "HH:MM"
 */
export const formatMatchDateTime = (dateTime: string | Date): [string, string] => {
  const date = new Date(dateTime);

  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const formattedDate = `${day}/${month}`;

  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  const formattedTime = `${hours}:${minutes}`;

  return [formattedDate, formattedTime];
};

/**
 * Formats only the date part (DD/MM format without year).
 * Uses the browser's local timezone.
 *
 * @param dateTime - ISO date string or Date object
 * @returns Date string in "DD/MM" format
 */
export const formatMatchDate = (dateTime: string | Date): string => {
  const date = new Date(dateTime);
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  return `${day}/${month}`;
};

/**
 * Formats only the time part (HH:MM format).
 * Uses the browser's local timezone.
 *
 * @param dateTime - ISO date string or Date object
 * @returns Time string in "HH:MM" format
 */
export const formatMatchTime = (dateTime: string | Date): string => {
  const date = new Date(dateTime);
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
};

/**
 * Truncates a text to a specified length
 * @param text Text to truncate
 * @param maxLength Maximum length before truncating
 * @returns Truncated text with ellipsis if needed
 */
export function truncateText(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.slice(0, maxLength) + '...';
}

/**
 * Generates a unique ID
 * @returns A unique string ID
 */
export function generateId(): string {
  return Math.random().toString(36).substring(2) + Date.now().toString(36);
}

export const createRuleBlock = (
  html: string,
  ruleId?: string,
  order?: number,
): string => {
  const sanitizedHtml = DOMPurify.sanitize(html).trim();

  const plainText = sanitizedHtml
    .replace(/<[^>]*>/g, "")
    .replace(/&nbsp;/g, " ")
    .trim();

  if (!plainText) return "";

  const id = ruleId ?? crypto.randomUUID();
  const orderAttr =
    order != null && order > 0 ? ` data-rule-order="${order}"` : "";

  return `<div class="rules-item" data-rule-id="${id}"${orderAttr}>${sanitizedHtml}</div>`;
};

export const parseRulesFromHtml = (
  html: string,
  sectionId = "",
): RuleItem[] => {
  if (!html.trim()) return [];

  const wrapper = document.createElement("div");
  wrapper.innerHTML = html;

  const rules = Array.from(wrapper.querySelectorAll(".rules-item")).map(
    (rule, index) => {
      const parsedOrder = Number.parseInt(
        rule.getAttribute("data-rule-order") || "",
        10,
      );

      return {
        id: rule.getAttribute("data-rule-id") || crypto.randomUUID(),
        html: rule.innerHTML,
        text:
          (rule.textContent || "").replace(/\u00A0/g, " ").trim() ||
          "Untitled rule",
        sectionId,
        order:
          Number.isFinite(parsedOrder) && parsedOrder > 0
            ? parsedOrder
            : index + 1,
      };
    },
  );

  return rules.sort((a, b) => a.order - b.order);
};