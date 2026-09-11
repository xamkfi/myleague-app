import { userService } from '../../../api/admin/userService';
import type { ClubAdminSelection } from './ClubAdminsPicker';

/**
 * Ensures every selected club admin has a user account, inviting persons who
 * do not have one yet, then returns the user IDs to persist on the club.
 */
export async function resolveClubAdminUserIds(
  admins: ClubAdminSelection[],
  clubId: string,
): Promise<string[]> {
  const userIds: string[] = [];

  for (const admin of admins) {
    if (admin.userId) {
      userIds.push(admin.userId);
      continue;
    }

    const created = await userService.create({
      email: admin.email,
      personId: admin.personId,
      role: 'ClubAdmin',
      clubAssignments: [clubId],
    });
    userIds.push(created.id);
  }

  return userIds;
}
