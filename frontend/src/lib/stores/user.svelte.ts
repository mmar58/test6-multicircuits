export class UserState {
    name: string = "";
    id: string = "";
    color: string = "";
    isJoined: boolean = false;
}

export const userStore = $state(new UserState());
