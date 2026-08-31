import { useEffect, useState } from 'react';
import type { ProjectTaskDto } from '@taskmanager/shared';
import { castVote, getErrorMessage } from '@taskmanager/shared';
import { useVoteControlStyles } from './VoteControl.styles';

const VOTE_VALUES = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] as const;

interface VoteControlProps {
    task: ProjectTaskDto;
    currentUserId: string;
}

export function VoteControl({ task, currentUserId }: VoteControlProps) {
    const styles = useVoteControlStyles();

    const [myVote, setMyVote] = useState<number | null>(task.votes[currentUserId] ?? null);
    const [weightedScore, setWeightedScore] = useState(task.weightedScore);
    const [voting, setVoting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        setMyVote(task.votes[currentUserId] ?? null);
        setWeightedScore(task.weightedScore);
    }, [task.id, task.votes, task.weightedScore, currentUserId]);

    const handleVote = async (value: number) => {
        setVoting(true);
        setError(null);
        try {
            const updated = await castVote(task.id, value);
            setMyVote(value);
            setWeightedScore(updated.weightedScore);
        } catch (err) {
            setError(getErrorMessage(err, 'Failed to cast vote'));
        } finally {
            setVoting(false);
        }
    };

    return (
        <>
            <div className={styles.labelRow}>
                <p className={styles.sectionLabel}>Your Vote</p>
                <span className={styles.aggregate}>Weighted score: {weightedScore}</span>
            </div>
            <div className={styles.pillRow}>
                {VOTE_VALUES.map(value => (
                    <button
                        key={value}
                        type="button"
                        disabled={voting}
                        onClick={() => void handleVote(value)}
                        className={`${styles.pillBase} ${myVote === value ? styles.pillActive : styles.pillInactive}`}
                    >
                        {value}
                    </button>
                ))}
            </div>
            {error && <p className={styles.fieldError}>{error}</p>}
        </>
    );
}
