
export type RecoveryPlan = {
    id: number;
    name: string;
    workoutDays: WorkoutDay[];
    isCreatedByDoctor: boolean;
};

export type WorkoutDay = {
    dayNumber: number;
    exercises: Exercise[]; 
};

export type Exercise = {
    id: number;
    name: string;
    description: string;
    sets: number;
    reps: number;
    duration: string | null; 
};

export type RecoveryPlanWithAppUserInfo = {
    id: number;
    name: string;
    workoutDays: WorkoutDay[];
    appUserId: string;
}
