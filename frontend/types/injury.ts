export type Injury = {
    id: number;
    name: string;
    description: string;
    recoveryExercises?: RecoveryExercise[];
    isTooSevere?: boolean;
    bodyPart: string;
}
